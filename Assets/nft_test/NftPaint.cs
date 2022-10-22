#nullable enable
#nullable disable warnings

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using static NftTest;

public class NftPaint : MonoBehaviour {

  [SerializeField]
  public NftMenu _nftMenu;

  private Material? material;
  private Texture? texture;

  private string? assetPolicyIdHex = null;
  private string? assetNameHex = null;
  private string? assetNameAscii = null;

  private string? imageIpfs = null;
  private string? imageUrl = null;

  void Awake() {
    material = GetComponent<Renderer>().material;
  }

  public void SetNftImage( string policyIdHex, string nameHex ) {
    this.assetPolicyIdHex = policyIdHex;
    this.assetNameHex = nameHex;
    this.assetNameAscii = HexToAscii( nameHex );

    imageIpfs = null;
    imageUrl = null;

    StartCoroutine( FetchImageUrl() );
  }

  IEnumerator FetchImageUrl() {
    using ( UnityWebRequest request = UnityWebRequest.Get( "https://api.koios.rest/api/v0/asset_info?_asset_policy=" + assetPolicyIdHex + "&_asset_name=" + assetNameHex ) ) {
      yield return request.SendWebRequest();

      if ( request.result == UnityWebRequest.Result.ConnectionError ) {
        Debug.Log( request.error );
        _nftMenu.EnableInteract();
      } else {
        string jsonStr = request.downloadHandler.text;
        Tuple<string?, string?>? assetInfo = ParseAssetInfo( jsonStr, assetPolicyIdHex, assetNameHex );
        if ( assetInfo is null || assetInfo.Item2 is null ) {
          _nftMenu.EnableInteract();
          yield break;
        }

        imageIpfs = assetInfo.Item2.ToString();
        imageUrl = GetIpfsImageUrl( imageIpfs );

        StartCoroutine( GetTexture() );
      }
    }
  }

  IEnumerator GetTexture() {
    using ( UnityWebRequest request = UnityWebRequestTexture.GetTexture( imageUrl ) ) {
      yield return request.SendWebRequest();

      if ( request.result == UnityWebRequest.Result.ConnectionError ) {
        Debug.Log( request.error );
        _nftMenu.EnableInteract();
      } else {
        texture = ( ( DownloadHandlerTexture ) request.downloadHandler ).texture;
        if ( material is not null ) material.SetTexture( "_MainTex", texture );
      }

      _nftMenu.SetSelectedNft( assetPolicyIdHex, assetNameHex );
      _nftMenu.EnableInteract();
    }
  }

}
