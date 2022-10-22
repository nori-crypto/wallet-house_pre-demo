#nullable enable
#nullable disable warnings

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using static NftTest;

public class NftMenu : MonoBehaviour {

  [SerializeField]
  private Canvas _nftCanvas;

  [SerializeField]
  private GameObject _scrollViewContent;

  [SerializeField]
  private TMP_InputField _walletAddressInput;

  [SerializeField]
  private Button _loadWalletButton;

  [SerializeField]
  private NftInfoPanel nftInfoPanelPrefab;

  [SerializeField]
  public NftPaint _nftPaint;

  public void EnableInteract() {
    _walletAddressInput.interactable = true;
    _loadWalletButton.interactable = true;
    for ( int i = 0 ; i < _scrollViewContent.transform.childCount ; i++ ) {
      _scrollViewContent.transform.GetChild( i ).GetComponent<Button>().interactable = true;
    }
  }

  public void DisableInteract() {
    _walletAddressInput.interactable = false;
    _loadWalletButton.interactable = false;
    for ( int i = 0 ; i < _scrollViewContent.transform.childCount ; i++ ) {
      _scrollViewContent.transform.GetChild( i ).GetComponent<Button>().interactable = false;
    }
  }

  public void OnClickLoadWallet() {
    DisableInteract();

    while ( _scrollViewContent.transform.childCount > 0 ) {
      DestroyImmediate( _scrollViewContent.transform.GetChild(0).gameObject );
    }

    string walletAddress = _walletAddressInput.text;

    StartCoroutine( FetchWalletAssets( walletAddress ) );
  }

  IEnumerator FetchWalletAssets( string address ) {
    byte[] data = Encoding.UTF8.GetBytes( "{ \"_addresses\": [ \""+ address + "\" ] }" );
    using ( UnityWebRequest request = new UnityWebRequest( "https://api.koios.rest/api/v0/address_assets", UnityWebRequest.kHttpVerbPOST ) {
      uploadHandler   = new UploadHandlerRaw( data ),
      downloadHandler = new DownloadHandlerBuffer()
    } ) {
      request.SetRequestHeader( "Content-Type", "application/json" );
      yield return request.SendWebRequest();

      if ( request.result == UnityWebRequest.Result.ConnectionError ) {
        Debug.LogError( request.error );
        _walletAddressInput.interactable = true;
        _loadWalletButton.interactable = true;
      } else {
        string jsonStr = request.downloadHandler.text;

        List<Tuple<Tuple<string, string>,ulong>>? addressAssets = ParseAddressAssetsInfo( jsonStr );
        if ( addressAssets is null ) {
          EnableInteract();
          yield break;
        }

        foreach ( Tuple<Tuple<string, string>,ulong> assetInfo in addressAssets ) {
          NftInfoPanel nftInfoPanel = Instantiate<NftInfoPanel>( nftInfoPanelPrefab );
          nftInfoPanel.transform.SetParent( _scrollViewContent.transform, false );
          nftInfoPanel._nftMenu = this;
          nftInfoPanel.SetNftInfo( assetInfo.Item1.Item1, assetInfo.Item1.Item2 );
        }

        EnableInteract();
      }
    }
  }

  public void SetSelectedNft( string policyIdHex, string nameHex ) {
    SetAllUnselectedNft();
    for ( int i = 0 ; i < _scrollViewContent.transform.childCount ; i++ ) {
      NftInfoPanel nftInfoPanel = _scrollViewContent.transform.GetChild( i ).GetComponent<NftInfoPanel>();
      if ( nftInfoPanel.policyIdHex == policyIdHex && nftInfoPanel.nameHex == nameHex ) {
        nftInfoPanel.SetAsSelected();
      }
    }
  }

  public void SetAllUnselectedNft() {
    for ( int i = 0 ; i < _scrollViewContent.transform.childCount ; i++ ) {
      _scrollViewContent.transform.GetChild( i ).GetComponent<NftInfoPanel>().SetAsNotSelected();
    }
  }

}
