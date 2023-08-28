require "http/client"
require "json"
require "../models/lake.cr"

ENV["API_URL"] ||= "https://api.woog.life"

class ApiException < Exception
end

struct Response
  include JSON::Serializable

  property lakes : Array(LakeItem)

  def initialize(@lakes : Array(LakeItem))
  end
end

def get_lakes() : Response
  response = HTTP::Client.get "#{ENV["API_URL"]}/lake"

  if response.status_code == 200
    Response.from_json(response.body)
  else
    raise ApiException.new("failed to retrieve list of lakes")
  end
end


def get_tides_by_uuid(uuid : String, at : String, upcomingLimit : Int) : Array(Tide)
    response = HTTP::Client.get "#{ENV["API_URL"]}/lake/#{uuid}/tides"
  if response.status_code == 200
    api_lake = ApiTides.from_json(response.body)
    api_lake.extrema
  else
    raise ApiException.new("failed to get lake for #{uuid}")
  end
end


def get_lake_by_uuid(uuid : String, precision = 1, formatRegion = nil) : Lake
  response = HTTP::Client.get "#{ENV["API_URL"]}/lake/#{uuid}"
  if response.status_code == 200
    api_lake = ApiLake.from_json(response.body)
    get_lake(api_lake, precision, formatRegion)
  else
    raise ApiException.new("failed to get lake for #{uuid}")
  end
end


def get_lake(lake : ApiLake, precision = 1, formatRegion = nil) : Lake
  url = "#{ENV["API_URL"]}/lake/#{lake.id}/temperature?precision=#{precision}"
  if formatRegion != nil
    url = url + "&formatRegion=#{formatRegion}"
  end
  response = HTTP::Client.get url

  if response.status_code == 200
    data = LakeData.from_json(response.body)
    Lake.new(lake.id, lake.name, data)
  else
    raise ApiException.new("failed to get lake data")
  end
end
