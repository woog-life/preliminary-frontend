# see https://github.com/woog-life/api/issues/50
def get_country_code_from_header(accept_language_value : String?) : String?
  if accept_language_value == nil
    return nil
  end

  language : String = accept_language_value.as(String)
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
