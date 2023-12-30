require "html"
require "json"

HTML_REPLACEMENTS = {
  "ä" => "&auml;",
  "Ä" => "&Auml;",
  "ö" => "&ouml;",
  "Ö" => "&Ouml;",
  "ü" => "&uuml;",
  "Ü" => "&Uuml;",
  "ß" => "&szlig;",
}

PATH_REPLACEMENTS = {
  "ä" => "ae",
  "Ä" => "ae",
  "ö" => "oe",
  "Ö" => "oe",
  "ü" => "ue",
  "Ü" => "ue",
  "ß" => "ss",
}

struct LakeData
  include JSON::Serializable

  property time : String
  property temperature : Int8
  property preciseTemperature : String
end

struct ApiLake
  include JSON::Serializable

  property id : String
  property name : String
  property timeZoneId : String
end

struct Tide
  include JSON::Serializable

  property time : String
  property height : String
  property isHighTide : Bool

  def formatted_time(timeZoneId : String)
    t = Time::Format::ISO_8601_DATE_TIME.parse(@time)
    t = t.in Time::Location.load(timeZoneId)

    t.to_s "%H:%M %d.%m"
  end
end

struct ApiTides
  include JSON::Serializable

  property extrema : Array(Tide)
end

struct Lake
  include JSON::Serializable

  property id : String
  property name : String
  property data : LakeData
  property timeZoneId : String

  def initialize(@id : String, @name : String, @data : LakeData, @timeZoneId : String)
  end

  def formatted_time()
    t = Time::Format::ISO_8601_DATE_TIME.parse(@data.time)
    t = t.in Time::Location.load(@timeZoneId)

    t.to_s "%H:%M %d.%m.%Y"
  end

  def town()
    @name.split("(")[1].strip(")")
  end

  def html_name()
    name = HTML.escape @name
    HTML_REPLACEMENTS.each do |key, value|
      name = name.gsub(key, value)
    end

    name
  end
end

struct LakeItem
  include JSON::Serializable

  property id : String
  property name : String
  property supportedFeatures : Array(String)
  property timeZoneId : String

  def initialize(@id : String, @name : String, @supportedFeatures : Array(String), @timeZoneId : String)
  end

  def html_name()
    name = HTML.escape @name
    HTML_REPLACEMENTS.each do |key, value|
      name = name.gsub(key, value)
    end

    name
  end

  def has_feature(feature : String)
    found = @supportedFeatures.index feature
    !found.nil?
  end

  def path_name()
    @id
  end
end
