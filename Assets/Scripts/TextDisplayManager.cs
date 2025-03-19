using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class TextDisplayManager : MonoBehaviour
{
    public static TextDisplayManager Instance { get; private set; }

    [SerializeField]
    private TextMeshProUGUI _textDisplayCenter;
    
    [SerializeField]
    private TextMeshProUGUI _textDisplay;

    private Coroutine _textDisplayCoroutine = null;
    private const int MaxLineOfTextDisplay = 3;
    
    void Start()
    {
        if(Instance == null){
            Instance = this;
        }
        else{
            Destroy(this);
        }
    }
    
    public static bool WriteTextCenter(string text, float intervalSeconds = 0.1f, bool doClear = true) {
        if(Instance is null) return false;

        return Instance.WriteTextImpl(Instance._textDisplayCenter, text, intervalSeconds, doClear);
    }

    public static bool WriteText(string text, float intervalSeconds = 0.1f, bool doClear = true) {
        if(Instance is null) return false;

        return Instance.WriteTextImpl(Instance._textDisplay, text, intervalSeconds, doClear);
    }

    public bool WriteTextImpl(TextMeshProUGUI textMesh, string text, float intervalSeconds, bool doClear){
        if(_textDisplayCoroutine is not null){
            return false;
        }
        
        if(doClear) ClearTextImpl(textMesh);
        else if(!string.IsNullOrEmpty(textMesh.text)) textMesh.text += '\n';

        if(intervalSeconds == 0f){
            if(textMesh == _textDisplay){
                var lines = textMesh.text.Split('\n').Where(l => !string.IsNullOrEmpty(l)).ToList();
                if(lines.Count == MaxLineOfTextDisplay)
                    lines.RemoveAt(0);
                lines.Add(text);

                textMesh.text = string.Join('\n', lines);    
            }
            else{
                textMesh.text += text;
            }
            
            return true;
        }
        
        if(_textDisplayCoroutine is not null) return false;
        _textDisplayCoroutine = StartCoroutine(CoWriteText(textMesh, text, intervalSeconds));
        return true;
    }

    private IEnumerator CoWriteText(TextMeshProUGUI textMesh, string text, float intervalSeconds){
        if(textMesh == _textDisplay){
            var lines = textMesh.text.Split('\n').Where(l => !string.IsNullOrEmpty(l)).ToList();
            if(lines.Count == MaxLineOfTextDisplay)
                lines.RemoveAt(0);
            
            if(lines.Count != 0)
                textMesh.text = string.Join('\n', lines) + '\n';
        }
        
        foreach(var c in text){
            textMesh.text += c;

            yield return new WaitForSeconds(intervalSeconds);
        }

        _textDisplayCoroutine = null;
        yield return null;
    }

    private bool ClearTextImpl(TextMeshProUGUI textMesh){
        textMesh.text = "";

        return true;
    }
    public static bool ClearTextCenter(){
        if(Instance is null) return false;
        
        Instance.ClearTextImpl(Instance._textDisplayCenter);
        
        return true;
    }
    public static bool ClearText(){
        if(Instance is null) return false;
        
        Instance.ClearTextImpl(Instance._textDisplay);
        
        return true;
    }

}
