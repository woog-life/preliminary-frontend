require "kemal"
require "./models/lake.cr"
require "./api/lake.cr"
require "./helper.cr"

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
    response : Response = get_lakes()
    lakes : Array(LakeItem) = response.lakes

    precision = 2
    formatRegion : String? = get_country_code_from_header(env.request.headers["Accept-Language"]?)

    begin
      get_lake_by_uuid(env.params.url["uuid"], precision, formatRegion).try { |current_lake |
        env.response.cookies << initial_lake_uuid_cookie(env.params.url["uuid"])
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
