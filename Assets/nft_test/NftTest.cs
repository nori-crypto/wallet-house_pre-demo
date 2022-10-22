#nullable enable
#nullable disable warnings

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public static class NftTest {

  public static JsonNode ParseJson( string jsonStr ) {
    return JsonNode.Parse( "{ \"root\": " + jsonStr + " }" );
  }

  public static string HexToAscii( string hex_str ) {
    int length = hex_str.Length / 2;
    byte[] bytes = new byte[length];
    for ( int i = 0, j = 0 ; i < length ; i++, j+= 2 ) {
      bytes[i] = System.Convert.ToByte( hex_str.Substring( j, 2 ), 16 );
    }
    string name = Encoding.GetEncoding( "utf-8" ).GetString( bytes );
    return name;
  }

  public static string AsciiToHex( string name ) {
    byte[] bytes = Encoding.UTF8.GetBytes( name );
    StringBuilder sb = new StringBuilder( bytes.Length * 2 );
    for ( int i = 0 ; i < bytes.Length ; i++ ) {
      sb.Append( bytes[i].ToString( "x2" ) );
    }
    return sb.ToString();
  }

  public static string? GetIpfsImageUrl( string ipfs ) {
    Match match = Regex.Match( ipfs, "ipfs://([0-9A-Za-z]+)" );
    if ( match.Groups.Count == 2 ) {
      string id = match.Groups[1].ToString();
      string url = "https://ipfs.io/ipfs/" + id;
      return url;
    } else {
      return null;
    }
  }

  public static List<Tuple<Tuple<string, string>,ulong>>? ParseAddressAssetsInfo( string jsonStr ) {
    if ( jsonStr is null || jsonStr == "" ) return null;

    List<Tuple<Tuple<string, string>,ulong>> result = new List<Tuple<Tuple<string, string>,ulong>>();

    try {
      JsonNode json = ParseJson( jsonStr );

      IList<object> data = json["root"][0]["assets"].Get<IList<object>>();

      foreach ( IDictionary<string, object> policy in data ) {
        string policyIdHex = (string) policy["policy_id"];
        foreach ( IDictionary<string, object> asset in (IList<Object>) policy["assets"] ) {
          string assetNameHex = (string) asset["asset_name"];
          ulong balance = UInt64.Parse( (string) asset["balance"] );
          result.Add( new Tuple<Tuple<string, string>,ulong>( new Tuple<string, string>( policyIdHex, assetNameHex ), balance ) );
        }
      }
    } catch {
      return null;
    }

    return result;
  }

  public static Tuple<string?, string?>? ParseAssetInfo( string jsonStr, string policyIdHex, string assetNameHex ) {
    if ( policyIdHex is null || assetNameHex is null || jsonStr is null ) return null;
    if ( policyIdHex == "" || assetNameHex == "" || jsonStr == "" ) return null;

    string assetNameAscii = HexToAscii( assetNameHex );

    Tuple<string?, string?>? result = null;

    try {
      JsonNode json = ParseJson( jsonStr );

      IDictionary<string, object> data = json["root"][0]["minting_tx_metadata"]["721"].Get<IDictionary<string, object>>();
      if ( ! data.ContainsKey( policyIdHex ) ) return null;
      IDictionary<string, object> datum = (IDictionary<string, object>) data[policyIdHex];
      if ( ! datum.ContainsKey( assetNameAscii ) ) return null;
      IDictionary<string, object> info = (IDictionary<string, object>) datum[assetNameAscii];

      string? name = null;
      string? image = null;

      if ( info.ContainsKey( "name" ) ) name = (string) info["name"];
      if ( info.ContainsKey( "image" ) ) image = (string) info["image"];

      result = new Tuple<string?, string?>( name, image );
    } catch {
      return null;
    }

    return result;
  }

}
