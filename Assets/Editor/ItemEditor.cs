using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemEditor : EditorWindow
{
    public static List<ItemInfo> itemInfoList=new List<ItemInfo>();
    //데이터 담을 리스트
    private VisualTreeAsset itemPrefab;
    //프리팹uxml
    
    private ListView itemViewList;
    //리스트뷰
    private VisualElement itemList;
    //아이템 리스트
    private Sprite basicItemIcon;
    //기본아이콘

    private readonly float itemHeight = 40;
    //아이템 하나마다 리스트뷰 높이가 커져야하기 때문
    
    private ScrollView itemScrollView;
    //Infos 스크롤뷰
    private VisualElement ItemIcon;
    //큰 아이콘
    private ItemInfo currentItemInfo;
    //현재 선택한 아이템데이터
    
    [MenuItem("Tools/ItemEditor")]
    public static void ShowExample()
    {
        ItemEditor wnd = GetWindow<ItemEditor>();
        //불러옵니다.
        wnd.titleContent = new GUIContent("ItemEditor");
        Vector2 size = new Vector2(1000f,400f);
        wnd.minSize = size;
        wnd.maxSize = size;
        //사이즈 설정


    }

    private void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        //최상의 화면을 변수 root로 설정합니다
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset> 
            ("Assets/Editor/ItemEditor.uxml");
        rootVisualElement.Add(visualTree.Instantiate());
        //uxml를 불러옵니다.
        
        itemPrefab = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/itemprefab.uxml");
        LoadData();
        itemList = rootVisualElement.Q<VisualElement>("ItemList");
        basicItemIcon = (Sprite)AssetDatabase.LoadAssetAtPath(
             "Assets/RPG_inventory_icons/f.png", typeof(Sprite));
         CreateListView();
         
         itemScrollView = rootVisualElement.Q<ScrollView>("Infos");
         //스크롤뷰를 가져옵니다.
         itemScrollView.style.visibility = Visibility.Hidden;
         //맨처음에는 정보를 숨겨야합니다.
         ItemIcon = itemScrollView.Q<VisualElement>("Icon");
         //아이템 아이콘을 가져옵니다.
         rootVisualElement.Q<Button>("NewBtn").clicked += NewBtn_Click;
         rootVisualElement.Q<Button>("DelBtn").clicked += DelBtn_Click;
         
         itemScrollView.Q<TextField>("Name")
             .RegisterValueChangedCallback(evt =>
             {
                 currentItemInfo.name = evt.newValue;
                 //이름 변경
                 itemViewList.Rebuild();
                 //갱신

             });
         itemScrollView.Q<ObjectField>("IconFiled")
             .RegisterValueChangedCallback(evt =>
             {
                 Sprite newSprite = evt.newValue as Sprite;
                 //아이콘을 가져옵니다.
                 currentItemInfo.icon = newSprite == null ? basicItemIcon : newSprite;
                 //아무것도없다면 기본으로 아니라면 아이콘을 수정합니다.
                 ItemIcon.style.backgroundImage =
                     newSprite == null ? basicItemIcon.texture : newSprite.texture;
                
                 itemViewList.Rebuild();
                 //갱신
             });
    }

    private void CreateListView()
    {
        Func<VisualElement> makeItem = () => itemPrefab.CloneTree();
        //우리가 프리팹으로 만든 uxml 

        Action<VisualElement, int> bindItem = (e, i) =>
        //바인딩 할목록은 아이콘과 이름만 필요
        {
            e.Q<VisualElement>("Icon").style.backgroundImage =
                itemInfoList[i] == null ? basicItemIcon.texture : itemInfoList[i].icon.texture;
            //만약에 아이콘이 null이라면 기본 아이콘으로 설정하고 아니면 해당 아이콘으로 설정한다.
            e.Q<Label>("Name").text = itemInfoList[i].name;
            //이름을 설정한다.
        };

        itemViewList = new ListView(itemInfoList, 40, makeItem, bindItem);
        //ListView를 만듭니다.
        
        itemViewList.selectionType = SelectionType.Single;
        //선택은 하나만 되게 설정.
        
        itemViewList.style.height = itemInfoList.Count * itemHeight;
        //리스트뷰의 높이를 아이템갯수에 맞춰서 높이를 설정합니다.
        itemList.Add(itemViewList);
        itemViewList.onSelectionChange += SelecFunc;
        
        
        
    }

    void NewBtn_Click()
    {
        ItemInfo Item = CreateInstance<ItemInfo>();
        
        Item.name = "ItemInfo";
        //기본 이름
        Item.icon = basicItemIcon;
        //기본 아이콘
        
        AssetDatabase.CreateAsset(Item,$"Assets/Data/Item/{Item.id}.asset");
        //생성시킵니다.
        itemInfoList.Add(Item);
        //리스트안에 넣습니다.
        itemViewList.Rebuild();
        //갱신시켜줍니다. 화면에 보이게
        itemViewList.style.height = itemInfoList.Count * itemHeight;
        //크기를 조절합니다.
        
    }

    void DelBtn_Click()
    {
        if (currentItemInfo==null)
        {
            return;
        }
        string path = AssetDatabase.GetAssetPath(currentItemInfo);
        //현재 선택된 아이템 경로를 가져옵니다.
        AssetDatabase.DeleteAsset(path);
        //삭제
        itemInfoList.Remove(currentItemInfo);
        //리스트도 삭제
        itemViewList.Rebuild();
        //리스트 갱신. 화면에 보이게
        itemScrollView.style.visibility = Visibility.Hidden;
        //정보창은 안보이게 꺼짐.
        currentItemInfo = null;
    }
    private void SelecFunc(IEnumerable<object> selectedItems)
    {
        currentItemInfo = (ItemInfo)selectedItems.First();
        //리스트뷰는 여러개 선택이 가능하지만 우리는 싱글로 선택하게 했습니다.
        //선택된것중에 첫번째를 가져옵니다.
        
        
        SerializedObject so = new SerializedObject(currentItemInfo);
        itemScrollView.Bind(so);
        //바인딩해줍니다.

        if (currentItemInfo.icon !=null)
        {
            ItemIcon.style.backgroundImage = currentItemInfo.icon.texture;
            //아이콘 설정해줍니다.
        }

        itemScrollView.style.visibility = Visibility.Visible;
        //이제 보여줘야합니다.

    }
    private void LoadData()
    {
        itemInfoList.Clear();
        //리스트 정리
        string[] Items = Directory.GetFiles("Assets/Data/Item", "*.asset", 
            SearchOption.AllDirectories);
        //모든 경로를 가져옵니다.
        foreach (string item in Items)
        {
            
            string cleanedPath = item.Replace("\\", "/");
            // \\를 / 로 바꿔줍니다. Assets\\Data 가 아닌 Assets/Data 로 쓰기때문에
            itemInfoList.Add((ItemInfo)AssetDatabase.LoadAssetAtPath(cleanedPath,
                typeof(ItemInfo)));
            //리스트안에 넣어줍니다. ItemInfo는 우리가 만든 스크립터블 오브젝트 입니다.
        }
        
    }
}
