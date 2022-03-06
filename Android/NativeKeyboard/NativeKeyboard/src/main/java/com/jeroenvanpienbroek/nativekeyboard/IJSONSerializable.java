package com.jeroenvanpienbroek.nativekeyboard;

import org.json.JSONObject;

public interface IJSONSerializable
{
    public void parseJSON(JSONObject jsonObject);
    public JSONObject toJSON();
}
