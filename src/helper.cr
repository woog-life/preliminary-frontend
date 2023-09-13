# see https://github.com/woog-life/api/issues/50
def get_country_code_from_header(accept_language_value : String?) : String?
  if accept_language_value == nil
    return nil
  end

  language : String = accept_language_value.as(String)

  # see the following two RFCs for the specification:
  # https://www.rfc-editor.org/rfc/rfc3282 (Content Language Headers)
  # https://www.rfc-editor.org/rfc/rfc3066 (Tags for the Identification of Languages)
  # note that while rfc3066 is obsoleted by rfc4646 + rfc4647 this is not reflected in RFC3282 (which isn't obsoleted)
  # thus we're using the definition from rfc3066
  primary_subtag = /(?<primary>[a-z]{1,8})/i
  subtag = /(?<subtag>[a-z0-9]{1,8})/i
  language_tag = /#{primary_subtag}(?:-#{subtag})?/i
  language_range = /(?<language_range>#{language_tag}|\*)/i
  qvalue = /(?<quality>0(?:\.[0-9]{1,3})?|1(?:\.0{1,3})?)/
  obs_language_q = /^(?:#{language_range}(?: ?; ?q ?=#{qvalue})?)$/

  # the actual obs_accept_language regex is the following:
  # obs_accept_language = / ?#{obs_language_q}(, ?#{obs_language_q})* ?/
  # we're going to split the string instead (on `, ?`) since this makes using capture groups a lot easier
  languages = language.split(/, ?/)

  highest_q_value = 0.0
  subtag = nil

  # sort by highest qvalue
  languages.each do |language|
    if value = language.match(obs_language_q)
      # "If a parameter has a quality value of 0, then content with this parameter is `not acceptable' for the client."
      # see https://www.rfc-editor.org/rfc/rfc2616#section-3.9
      begin
        quality = value["quality"].to_f?
      rescue KeyError
      end
      if quality.nil?
        begin
          subtag = value["subtag"]
          break
        rescue KeyError
          # process rest of the list
          next
        end
      end

      if quality != 0.0
        if highest_q_value == nil || highest_q_value < quality
          begin
            subtag = value["subtag"]
            highest_q_value = quality
          rescue KeyError
          # process rest of the list
          end
        end
      end
    else
      nil
    end
  end

  subtag
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
