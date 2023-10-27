require "kemal"
require "./models/lake.cr"
require "./api/lake.cr"
require "./helper.cr"
require "i18n"

I18n.config.loaders << I18n::Loader::YAML.new("config/locales")
I18n.config.available_locales = [:de, :en]
I18n.config.default_locale = :de
I18n.config.fallbacks = [:de]
I18n.init

def uuid_has_feature(lakes : Array(LakeItem), uuid : String, feature : String)
  lake : LakeItem | Nil = lakes.find { |lake| lake.id == uuid }
  if lake
    lake.has_feature feature
  else
    false
  end
end

module Frontend
  VERSION = "0.1.0"

  error 404 do |env|
    env.redirect "/"
  end

  get "/" do |env|
    uuid = "69c8438b-5aef-442f-a70d-e0d783ea2b38"
    if initial_uuid_cookie = env.request.cookies["initial-lake-uuid"]?
      uuid = initial_uuid_cookie.value
    end
    env.redirect "/#{uuid}"
  end

  # TODO: accept query params precision and formatRegion
  get "/:uuid" do |env|
    # TODO: cache lakes
    lakesChannel = Channel(Array(LakeItem)).new
    tidesChannel = Channel(Array(Tide)).new

    spawn do
      response : Response = get_lakes()
      lakes : Array(LakeItem) = response.lakes
      lakesChannel.send(lakes)
    end

    precision = 2
    acceptable_languages = parse_accept_language_header(env.request.headers["Accept-Language"]?)
    formatRegion : String = get_country_code_from_header(acceptable_languages, "DE")
    locale : String = get_language_from_header(acceptable_languages, I18n.config.default_locale)

    tides = [] of Tide

    begin
      lakeChannel = Channel(Lake).new
      spawn do
        lake : Lake = get_lake_by_uuid(env.params.url["uuid"], precision, formatRegion)
        lakeChannel.send(lake)
      end

      lakes = lakesChannel.receive
      hasTides = uuid_has_feature(lakes, env.params.url["uuid"], "tides")
      if hasTides
        spawn do
          tides = get_tides_by_uuid(env.params.url["uuid"])
          tidesChannel.send(tides)
        end
      end

      lakeChannel.receive.try { |current_lake|
        env.response.cookies << initial_lake_uuid_cookie(env.params.url["uuid"])

        if hasTides
          tides = tidesChannel.receive
        end

        I18n.locale = locale
        render "src/views/lake.ecr"
      }
    rescue ex : ApiException
      render "src/views/lake_error.ecr"
    end
  end

  static_headers do |response, filepath, filestat|
    if filepath =~ /\.(html|css|js|ttf)$/
      response.headers.add("Access-Control-Allow-Origin", "*")
    end
    response.headers.add("Content-Size", filestat.size.to_s)
  end
  serve_static({"gzip" => true, "dir_listing" => false})

  Kemal.run
end
