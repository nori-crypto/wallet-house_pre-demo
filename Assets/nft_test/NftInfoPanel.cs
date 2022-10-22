using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static NftTest;

public class NftInfoPanel : MonoBehaviour {

  public NftMenu _nftMenu;

  public Image _image;

  [SerializeField]
  private TMP_Text _policyText, _nameHexText, _nameAsciiText;

  private string _policyIdHex;
  public string policyIdHex { get { return _policyIdHex; } }
  private string _nameHex;
  public string nameHex { get { return _nameHex; } }

  void Awake() {
    _image = GetComponent<Image>();
  }

  public void SetNftInfo( string policyIdHex, string nameHex ) {
    this._policyIdHex = policyIdHex;
    this._nameHex = nameHex;

    _policyText.text = this._policyIdHex;
    _nameHexText.text = this._nameHex;
    _nameAsciiText.text = HexToAscii( this._nameHex );
  }

  public void OnClick() {
    _nftMenu.DisableInteract();
    _nftMenu._nftPaint.SetNftImage( _policyIdHex, _nameHex );
  }

  public void SetAsSelected() {
    _image.color = new Color( 0.5f, 1.0f, 1.0f, 1.0f );
  }

  public void SetAsNotSelected() {
    _image.color = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
  }

}
