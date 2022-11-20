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
    # TODO: read cookie
    env.redirect "/69c8438b-5aef-442f-a70d-e0d783ea2b38"
  end

  # TODO: accept query params precision and formatRegion
  get "/:uuid" do |env|
    # TODO: cache lakes
    response = get_lakes()
    lakes = response.lakes

    precision = 2
    formatRegion = get_country_code_from_header(env.request.headers["Accept-Language"])
    current_lake = get_lake_by_uuid(env.params.url["uuid"], precision, formatRegion)

    render "src/views/lake.ecr"
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
