using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="MemoData", menuName ="Story/MemoData")]
public class MemoData : ScriptableObject
{
    /*
    MemoUI Prefab Structure:
    - MemoUI (Canvas)
   - Background (Image)
    - TitleText (Text)
   - ContentText (Text)
   - CloseButton (Button)

    */

    public string title;
    public string content;

}
