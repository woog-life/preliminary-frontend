require "kemal"
require "./models/lake.cr"
require "./api/lake.cr"

module Frontend
  VERSION = "0.1.0"

  get "/" do
    # TODO: read cookie
    "<html><head><title>Hello World!</title><link rel=\"stylesheet\" href=\"/css/main.css\" media=\"screen\"><head></html>"
  end

  get "/:uuid" do |env|
    response = get_lakes()
    lakes = response.lakes
    current_lake = get_lake(env.params.url["uuid"])

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
