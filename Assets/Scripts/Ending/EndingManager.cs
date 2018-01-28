using Assets.Scripts.General;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour {

    public Text TextDecrypt;
    public Text Deaths;

	public void Start () {
        this.TextDecrypt.text = this.obfuscateMessage(Global.EmailMessage);
        this.Deaths.text = "Deaths : " + Global.deathCounter;
    }

    public string obfuscateMessage(string message)
    {
        int deaths = Global.deathCounter;
        if (deaths <= 0) return message;
        int obfuscate = deaths * 4;

        List<char> weirdLetters = new List<char>{'$','#','@','}','{','&','*','%','~'};
        StringBuilder msg = new StringBuilder(message);

        while(true)
        {
            if (obfuscate <= 0) break;
            int position = Random.Range(0, message.Length);
            char chr = msg[position];
            if (weirdLetters.Contains(chr) || chr == '\\') continue;
            msg[position] = weirdLetters[Random.Range(0, weirdLetters.Count)];
            obfuscate -= 1;
        }

        return msg.ToString();
    }
}
