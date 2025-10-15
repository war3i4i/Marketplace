using Marketplace.ExternalLoads;
using Marketplace.Modules.MainMarketplace;
using UnityEngine.EventSystems;

namespace Marketplace.Modules.CMS;

[Market_Autoload(Market_Autoload.Type.Client)]
public static class CMS_Links
{
    public const string KEY = "kg.CMSLINK:";
    public static GameObject TooltipPrefab;
    public static GameObject TooltipTrick;
    
    [UsedImplicitly]
    private static void OnInit()
    {
        TooltipPrefab = AssetStorage.asset.LoadAsset<GameObject>("TooltipPrefab_CMS");
        TooltipTrick = new GameObject("kg_CMS_TooltipTrick") { hideFlags = HideFlags.HideAndDontSave };
        TooltipTrick.AddComponent<UITooltip>(); 
        TooltipTrick.SetActive(false);
    }
    
    public class CMS_LinkController : MonoBehaviour
    {
        private static readonly Dictionary<int, Marketplace_DataTypes.ClientMarketSendData> _itemLinks = [];

        
        private int _lastLinkIndex = -1;
        private int linkStartTextIndex = -1;
        private int linkTextLength = 0;
        private bool changeColor => linkStartTextIndex != -1 && linkTextLength != 0;
        private static GameObject CurrentTooltip;
        private TMP_Text _text;
        
        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _text.raycastTarget = true;
        }
        private void OnDisable()
        {
            if (changeColor) ChangeVertexColors(false);
            linkStartTextIndex = -1;
            linkTextLength = 0;
            _lastLinkIndex = -1;
            if (CurrentTooltip) Destroy(CurrentTooltip);
        }
        public void SetItemLinks(List<Marketplace_DataTypes.ClientMarketSendData> itemLinks, List<Marketplace_DataTypes.ClientMarketSendData> itemAttachments) 
        {
            OnDisable();
            _itemLinks.Clear();
            if (itemLinks != null && itemLinks.Count > 0)
            {
                int startLinkIndex = 0;
                foreach (Marketplace_DataTypes.ClientMarketSendData itemLink in itemLinks)
                {
                    _itemLinks[startLinkIndex++] = itemLink;
                }
            }
            if (itemAttachments != null && itemAttachments.Count > 0)
            {
                int startLinkIndex = 100;
                foreach (Marketplace_DataTypes.ClientMarketSendData itemLink in itemAttachments)
                {
                    _itemLinks[startLinkIndex++] = itemLink;
                }
            }
        }
        private void ProcessTMP(int tryFindLink)
        {
            TMP_LinkInfo linkInfo = _text.textInfo.linkInfo[tryFindLink];
            string linkId = linkInfo.GetLinkID();
            if (linkId.Contains(KEY))
            {
                try
                {
                    int linkIndex = int.Parse(linkId.Split(':')[1]);
                    if (!_itemLinks.ContainsKey(linkIndex))
                    {
                        OnDisable();
                        return;
                    }
                    Marketplace_DataTypes.ClientMarketSendData itemLinkData = _itemLinks[linkIndex];
                    if (itemLinkData == null || _lastLinkIndex == linkIndex) return;
                    if (changeColor) ChangeVertexColors(false);
                    if (CurrentTooltip) Destroy(CurrentTooltip);
                    _lastLinkIndex = linkIndex;
                    linkStartTextIndex = linkInfo.linkTextfirstCharacterIndex;
                    linkTextLength = linkInfo.linkTextLength;
                    CurrentTooltip = CreateTooltipForItemLinkData(itemLinkData);
                    ChangeVertexColors(true);
                }  
                catch(Exception ex)
                { 
                    linkStartTextIndex = -1;
                    linkTextLength = 0;
                    Utils.print($"[CMS] Error: {ex}", ConsoleColor.Red);
                }
            }
        }
        private void LateUpdate()
        {
            if (!this.gameObject.IsHovering())
            {
                OnDisable();
                return;
            }
            int tryFindLink = TMP_TextUtilities.FindIntersectingLink(_text, Input.mousePosition, null);
            if (tryFindLink == -1)
            {
                OnDisable();
                return;
            }
            ProcessTMP(tryFindLink);
        }
        private GameObject CreateTooltipForItemLinkData(Marketplace_DataTypes.ClientMarketSendData itemLinkData,  string additionalText = "")
        {
            GetItemData(itemLinkData, out string hookName, out string hookText, out Sprite hookIcon, out ItemDrop.ItemData item);
            GameObject result = Instantiate(TooltipPrefab, transform.parent);
            result.transform.Find("Bkg/Icon").GetComponent<Image>().sprite = hookIcon;
            result.transform.Find("Bkg/Icon").GetComponent<Image>().type = Image.Type.Sliced;
            result.transform.Find("Bkg/Topic").GetComponent<Text>().text = Localization.instance.Localize(hookName) + additionalText;
            result.transform.Find("Bkg/Text").GetComponent<Text>().text = Localization.instance.Localize(hookText);
            Transform trannyHoles = result.transform.Find("Bkg/TrannyHoles");
            bool isJC = Jewelcrafting.API.FillItemContainerTooltip(item, trannyHoles.parent, false);
            trannyHoles.gameObject.SetActive(isJC);
            AdjustPosition(result.transform.GetChild(0).transform as RectTransform);
            return result;
        }
        public static void GetItemData(Marketplace_DataTypes.ClientMarketSendData itemLinkData, out string topic, out string text, out Sprite icon, out ItemDrop.ItemData item)
        {
            item = ObjectDB.instance.m_itemByHash[itemLinkData.ItemPrefab.GetStableHashCode()].GetComponent<ItemDrop>().m_itemData.Clone();
            item.m_customData = fastJSON.JSON.ToObject<Dictionary<string, string>>(itemLinkData.CUSTOMdata);
            item.m_stack = itemLinkData.Count;
            item.m_quality = itemLinkData.Quality;
            item.m_variant = itemLinkData.Variant;
            item.m_crafterID = itemLinkData.CrafterID;
            item.m_crafterName = itemLinkData.CrafterName;
            item.m_durability = item.GetMaxDurability() * itemLinkData.DurabilityPercent / 100f;
            
            UITooltip littleTrick = TooltipTrick.GetComponent<UITooltip>();
            bool canBeRepairedOld = item.m_shared.m_canBeReparied;
            item.m_shared.m_canBeReparied = false;
            InventoryGui.instance.m_playerGrid.CreateItemTooltip(item, littleTrick);
            item.m_shared.m_canBeReparied = canBeRepairedOld;
            topic = Localization.instance.Localize(littleTrick.m_topic);
            text = Localization.instance.Localize(littleTrick.m_text);
            icon = item.GetIcon(); 
        }
        private List<Color> _oldColors = null!;
        private Color defaultHoverColor = new Color(0.34f, 1f, 0.31f);
        private void ChangeVertexColors(bool isHover)
        {
            if (isHover) _oldColors = new List<Color>(linkTextLength);
            for (int i = 0; i < linkTextLength; ++i)
            {
                int charIndex = linkStartTextIndex + i;
                if (_text.textInfo.characterInfo[charIndex].character == ' ') 
                {
                    _oldColors.Add(defaultHoverColor);
                    _oldColors.Add(defaultHoverColor);
                    _oldColors.Add(defaultHoverColor);
                    _oldColors.Add(defaultHoverColor);
                    continue;
                }
                int meshIndex = _text.textInfo.characterInfo[charIndex].materialReferenceIndex;
                int vertexIndex = _text.textInfo.characterInfo[charIndex].vertexIndex;
                Color32[] vertexColors = _text.textInfo.meshInfo[meshIndex].colors32;
                if (isHover)
                { 
                    _oldColors.Add(vertexColors[vertexIndex + 0]);
                    _oldColors.Add(vertexColors[vertexIndex + 1]);
                    _oldColors.Add(vertexColors[vertexIndex + 2]);
                    _oldColors.Add(vertexColors[vertexIndex + 3]);
                    vertexColors[vertexIndex + 0] = defaultHoverColor;
                    vertexColors[vertexIndex + 1] = defaultHoverColor;
                    vertexColors[vertexIndex + 2] = defaultHoverColor;
                    vertexColors[vertexIndex + 3] = defaultHoverColor;
                }
                else
                {
                    vertexColors[vertexIndex + 0] = _oldColors[i * 4 + 0];
                    vertexColors[vertexIndex + 1] = _oldColors[i * 4 + 1];
                    vertexColors[vertexIndex + 2] = _oldColors[i * 4 + 2];
                    vertexColors[vertexIndex + 3] = _oldColors[i * 4 + 3];
                }
            }
            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
        private void AdjustPosition(RectTransform bkg, float minusX = 5f)
        {
            ContentSizeFitter sizeFitter = bkg.GetComponent<ContentSizeFitter>();   
            sizeFitter.enabled = false; 
            sizeFitter.enabled = true;
            Canvas.ForceUpdateCanvases();
            //Vector3 topLeft = _text.transform.TransformPoint(_text.textInfo.characterInfo[linkStartTextIndex].topLeft);
            //Vector3 spawnPos = new Vector3((topLeft.x + topRight.x) / 2f, topLeft.y, 0f);
            Vector3 topRight = _text.transform.TransformPoint(_text.textInfo.characterInfo[linkStartTextIndex + linkTextLength - 1].topRight);
            Vector3 bottomRight = _text.transform.TransformPoint(_text.textInfo.characterInfo[linkStartTextIndex + linkTextLength - 1].bottomRight);
            Vector3 spawnPos = new Vector3(topRight.x, (topRight.y + bottomRight.y) / 2f, 0f);
            bkg.transform.parent.position = spawnPos;
            (bkg.transform.parent as RectTransform).anchoredPosition += new Vector2(155 * 0.75f + minusX, bkg.sizeDelta.y / 2f * 0.75f);
        }
         
        
    } 
}
 

