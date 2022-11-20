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


def get_lake_by_uuid(uuid : String, precision = 1, formatRegion = "US")
  response = HTTP::Client.get "https://api.woog.life/lake/#{uuid}"
  if response.status_code == 200
    api_lake = ApiLake.from_json(response.body)
    get_lake(api_lake, precision, formatRegion)
  else
    raise ApiException.new("failed to get lake for #{uuid}")
  end
end


def get_lake(lake : ApiLake, precision = 1, formatRegion = "US")
  response = HTTP::Client.get "https://api.woog.life/lake/#{lake.id}/temperature?precision=#{precision}&formatRegion=#{formatRegion}"

  if response.status_code == 200
    data = LakeData.from_json(response.body)
    Lake.new(lake.id, lake.name, data)
  else
    raise ApiException.new("failed to get lake data")
  end
end