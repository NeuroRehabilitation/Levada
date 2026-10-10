using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class NatureTwinApi
{
    public Location location;
    public Current current;
    public Trends trends;
    public Today today;
    public Last24h last_24h;
    public DailyHistory daily_history;
    public AllTime all_time;
    public IpmaFallback ipma_fallback;
    public DataQuality data_quality;
    public List<Alert> alerts;
    public string generated_at;
}

[Serializable]
public class Location
{
    public int id;
    public string name;
    public float latitude;
    public float longitude;
    public string description;
    public int ipma_station_id;
    public string timezone;
    public bool enabled;
}

[Serializable]
public class Current
{
    public string recorded_at;
    public int minutes_ago;
    public float temperature_c;
    public float feels_like_c;
    public float dew_point_c;
    public float humidity_pct;
    public float pressure_mb;
    public float wind_kph;
    public float wind_gust_kph;
    public string wind_direction;
    public int wind_degree;
    public float precipitation_mm;
    public int cloud_cover_pct;
    public float uv_index;
    public float visibility_km;
    public string conditions;
}

[Serializable]
public class Trends
{
    public TrendData _1h;
    public TrendData _3h;
    public string pressure_tendency;
}

[Serializable]
public class TrendData
{
    public float temperature_change_c;
    public float humidity_change_pct;
    public float pressure_change_mb;
}

[Serializable]
public class Today
{
    public int readings_count;
    public TemperatureRange temperature_c;
    public HumidityRange humidity_pct;
    public float total_precipitation_mm;
    public int rainy_readings;
    public float avg_wind_kph;
    public float max_wind_gust_kph;
    public float max_uv_index;
    public float avg_pressure_mb;
    public string most_common_conditions;
}

[Serializable]
public class Last24h
{
    public int readings_count;
    public TemperatureRange temperature_c;
    public HumidityRange humidity_pct;
    public float total_precipitation_mm;
    public int rainy_readings;
    public float avg_wind_kph;
    public float max_wind_gust_kph;
    public float max_uv_index;
    public float avg_pressure_mb;
    public string most_common_conditions;
}

[Serializable]
public class TemperatureRange
{
    public float avg;
    public float min;
    public float max;
}

[Serializable]
public class HumidityRange
{
    public float avg;
    public float min;
    public float max;
}

[Serializable]
public class DailyHistory
{
    public int days_requested;
    public List<DailyRecord> days;
}

[Serializable]
public class DailyRecord
{
    public string date;
    public int readings_count;
    public float avg_temperature_c;
    public float min_temperature_c;
    public float max_temperature_c;
    public float avg_humidity_pct;
    public float total_precipitation_mm;
    public float max_wind_gust_kph;
}

[Serializable]
public class AllTime
{
    public int total_readings;
    public string first_reading;
    public string last_reading;
    public int days_with_data;
    public Records records;
}

[Serializable]
public class Records
{
    public RecordValue highest_temperature_c;
    public RecordValue lowest_temperature_c;
    public RecordValue strongest_gust_kph;
    public RecordValue highest_precipitation_mm;
    public RecordValue highest_uv_index;
}

[Serializable]
public class RecordValue
{
    public float value;
    public string recorded_at;
}

[Serializable]
public class IpmaFallback
{
    public bool used_last_24h;
    public object latest_forecast;
}

[Serializable]
public class DataQuality
{
    public int readings_last_24h;
    public int expected_last_24h;
    public float coverage_last_24h_pct;
    public string status;
}

[Serializable]
public class Alert
{
    public string type;
    public string message;
}
