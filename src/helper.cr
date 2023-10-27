alias PrimarySubtag = String
alias Subtag = String
struct LanguageTag
  property primary_subtag : PrimarySubtag
  property subtag : Subtag?

  def initialize(@primary_subtag : PrimarySubtag, @subtag : Subtag?)
  end
end
struct LanguageRange
  tag : LanguageTag?
  property accept_all : Bool

  def initialize(tag : LanguageTag?)
    if tag.nil?
      @tag = nil
      @accept_all = true
    else
      @tag = tag
      @accept_all = false
    end
  end

  def tag() : LanguageTag
    if @accept_all
      raise Exception.new("language range is of type `accept_all` (`*`)")
    end

    @tag.as(LanguageTag)
  end
end

alias QValue = Float64
struct ObsLanguageQ
  property language_range : LanguageRange
  property qvalue : QValue

  def initialize(@language_range : LanguageRange, @qvalue : QValue)
  end

  def has_subtag()
    !@language_range.tag?.subtag.nil?
  end
end

# see https://github.com/woog-life/api/issues/50
def parse_accept_language_header(accept_language_value : String?) : Array(ObsLanguageQ)?
  if accept_language_value == nil
    return nil
  end

  language : String = accept_language_value.as(String)

  # see the following two RFCs for the specification:
  # https://www.rfc-editor.org/rfc/rfc3282 (Content Language Headers)
  # https://www.rfc-editor.org/rfc/rfc3066 (Tags for the Identification of Languages)
  # note that while rfc3066 is obsoleted by rfc4646 + rfc4647 this is not reflected in RFC3282 (which isn't obsoleted)
  # thus we're using the definition from rfc3066
  # furthermore the new RFCs aren't using q-values anymore but a simple ordering:
  #  "The various matching operations described in this document include
  #   considerations for using a language priority list.  This document
  #   does not define the syntax for a language priority list; defining
  #   such a syntax is the responsibility of the protocol, application, or
  #   specification that uses it.  When given as examples in this document,
  #   language priority lists will be shown as a quoted sequence of ranges
  #   separated by commas, like this: "en, fr, zh-Hant" (which is read
  #   "English before French before Chinese as written in the Traditional
  #   script")."
  # in practice, q-values are still very much in use, since we're defaulting to a left-to-right priority in any case
  # (if no qvalues are given), we're somewhat compliant with the new priority list (not with the new definitions though)
  # while I'm not gonna implement the new specification, I've prepared the relevant regex definitions here:
  # alpha = /[a-z]/i
  # digit = /[0-9]/
  # alphanum = /([0-9a-z])/
  # script = /(?<script>#{alpha}{4})/
  # region = /(?<region>#{alpha}{2}|#{digit}{3)/
  # variant = /(?<variant>#{alphanum}{5,8}|(?:#{digit}#{alphanum}{3})?)/
  # singleton = /(?<singleton>[a-wy-z0-9])/i
  # extension = /(?<extension>#{singleton}(?:-(?:#{alphanum}{2,8)){1,}/i
  # privateuse = "(?<privateuse>(?:x|X)(?:-#{alphanum}{1,8}){1,})"
  # extlang = /(?<extlang>-#{alpha}}{3}){,3}/
  # language = /(?<language>#{alpha}{2,3}(#{extlang})?|#{alpha}{4}|#{alpha}{5,8})/
  # langtag = "(?<langtag>#{language}(?:-#{script})(?:-#{region})?(?:-#{variant})?(?:-#{extension})?(?:-#{privateuse})?)"
  # language_tag = "(?<language_tag>#{langtag}|#{privateuse}|#{grandfathered})"

  primary_subtag = /(?<primary>[a-z]{1,8})/i
  subtag = /(?<subtag>[a-z0-9]{1,8})/i
  language_tag = /#{primary_subtag}(?:-#{subtag})?/i
  language_range = /(?<language_range>#{language_tag}|\*)/i
  qvalue = /(?<quality>0(?:\.[0-9]{1,3})?|1(?:\.0{1,3})?)/
  # note that `^` and `$` are used here to force an exact match, when using `obs_accept_language`
  # these have to be removed
  obs_language_q = /^(?:#{language_range}(?: ?; ?q ?=#{qvalue})?)$/

  # the actual obs_accept_language regex is the following:
  # obs_accept_language = / ?#{obs_language_q}(, ?#{obs_language_q})* ?/
  # we're going to split the string instead (on `, ?`) since this makes using capture groups a lot easier
  languages = language.split(/, ?/)

  result = [] of ObsLanguageQ
  languages.each do |language|
    if value = language.match(obs_language_q)
      value = value.as(Regex::MatchData)
      begin
        quality = value["quality"].to_f
      rescue KeyError | ArgumentError
        # "The default value is q=1."
        # see https://www.rfc-editor.org/rfc/rfc2616#section-14.1
        # "If no Q values are given, the language-ranges are given in priority order,
        #  with the leftmost language-range being the most preferred language"
        # see https://www.rfc-editor.org/rfc/rfc3282#section-3
        quality = 1.0
      end


      # "If a parameter has a quality value of 0, then content with this parameter is `not acceptable' for the client."
      # see https://www.rfc-editor.org/rfc/rfc2616#section-3.9
      if quality == 0.0
        return
      end

      if value["language_range"] == "*"
        language_range = true
        language_tag = nil
      else
        primary_subtag = value["primary"]
        subtag = nil
        begin
          subtag = value["subtag"]
        rescue KeyError
        end
        language_tag = LanguageTag.new(primary_subtag, subtag)
      end
      language_range = LanguageRange.new(language_tag)

      result.push ObsLanguageQ.new(language_range, quality)
    end
  end

  result
end

def get_country_code_from_header(languages : Array(ObsLanguageQ)?, default : String) : String
  if languages.nil?
    return default
  end

  highest_q_value = 0.0
  language = nil

  languages.each do |lang|
    if highest_q_value == nil || highest_q_value < lang.qvalue
      begin
        language = lang
        highest_q_value = lang.qvalue
      rescue KeyError
        # process rest of the list
        next
      end
    end
  end

  if language.nil?
    default
  else
    if language.language_range.accept_all
      default
    elsif language.has_subtag?
      language.language_range.tag().subtag.as(String)
    else
      default
    end
  end
end

def get_language_from_header(languages : Array(ObsLanguageQ)?, default : String) : String
  if languages.nil?
    return default
  end

  highest_q_value = 0.0
  language = nil

  languages.each do |lang|
    if highest_q_value == nil || highest_q_value < lang.qvalue
      begin
        language = lang
        highest_q_value = lang.qvalue
      rescue KeyError
        # process rest of the list
        next
      end
    end
  end

  if language.nil?
    default
  else
    if language.language_range.accept_all
      default
    else
      language.language_range.tag().primary_subtag
    end
  end
end

def generate_cookie(name : String, value : String) : HTTP::Cookie
  HTTP::Cookie.new(
    name: name,
    value: value,
    secure: true,
    samesite: HTTP::Cookie::SameSite::Strict,
    http_only: true
  )
end

def initial_lake_uuid_cookie(uuid : String) : HTTP::Cookie
  generate_cookie("initial-lake-uuid", uuid)
end
