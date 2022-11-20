# see https://github.com/woog-life/api/issues/50
def get_country_code_from_header(accept_language_value : String)
  header_value_regex = /^((?<primary>\*|([A-Z]{1,8}))((?<!\*)-(?<subtag>[A-Z0-9]{1,8}))?)(;q=(?<quality>1|0|0.[0-9]{1,3}))?$/i
  if value = accept_language_value.match(header_value_regex)
    cc = value["subtag"]
    if cc == "DE" || cc == "US"
      cc
    else
      "US"
    end
  else
    "US"
  end
end

def generate_cookie(name : String, value : String)
  HTTP::Cookie.new(
    name: name,
    value: value,
    secure: true,
    samesite: HTTP::Cookie::SameSite::Strict,
    http_only: true
  )
end

def initial_lake_uuid_cookie(uuid : String)
  generate_cookie("initial-lake-uuid", uuid)
end
