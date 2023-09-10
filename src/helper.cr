# see https://github.com/woog-life/api/issues/50
def get_country_code_from_header(accept_language_value : String?) : String?
  if accept_language_value == nil
    return nil
  end

  language : String = accept_language_value.as(String)

  # see the following two RFCs for the specification: 
  # https://www.rfc-editor.org/rfc/rfc3282 (Content Language Headers)
  # https://www.rfc-editor.org/rfc/rfc3066 (Tags for the Identification of Languages)
  # note that while rfc3066 is obsoleted by rfc4646 + rfc4647 this is not reflected in RFC3282, thus we're using the definition from rfc3066
  header_value_regex = /^((?<primary>\*|([A-Z]{1,8}))((?<!\*)-(?<subtag>[A-Z0-9]{1,8}))?)(;q=(?<quality>1|0|0.[0-9]{1,3}))?$/i
  if value = language.match(header_value_regex)
    value["subtag"]
  else
    nil
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
