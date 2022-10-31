require "http/client"
require "json"
require "../models/lake.cr"

class ApiException < Exception
end

struct Response
  include JSON::Serializable

  property lakes : Array(LakeItem)

  def initialize(@lakes : Array(LakeItem))
  end
end

def get_lakes()
  response = HTTP::Client.get "https://api.woog.life/lake"

  if response.status_code == 200
    Response.from_json(response.body)
  else
    raise ApiException.new("failed to retrieve list of lakes")
  end
end


def get_lake(uuid : String, precision = 1)
  response = HTTP::Client.get "https://api.woog.life/lake/#{uuid}?precision=#{precision}"

  if response.status_code == 200
    Lake.from_json(response.body)
  else
    raise ApiException.new("failed to get lake information")
  end
end
