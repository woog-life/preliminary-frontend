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

struct Lake
  include JSON::Serializable

  property id : String
  property name : String
  property data : LakeData

  def formatted_time()
    t = Time::Format::ISO_8601_DATE_TIME.parse(@data.time, Time::Location.load("Europe/Berlin"))

    t.to_s "%H:%M %d.%m.%Y"
  end
end

struct LakeItem
  include JSON::Serializable

  property id : String
  property name : String
  property features : Array(String)

  def initialize(@id : String, @name : String, @features : Array(String))
  end

  def html_name()
    name = HTML.escape @name
    HTML_REPLACEMENTS.each do |key, value|
      name = name.gsub(key, value)
    end

    name
  end

  def path_name()
    # name = HTML.escape(@name.gsub(/(\s)/) {
    #   "_"
    # })
    # PATH_REPLACEMENTS.each do |key, value|
    #   name = name.gsub(key, value)
    # end
    #
    # name.downcase()
    @id
  end
end
