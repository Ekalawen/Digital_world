using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TresholdFragment {
    public int treshold;
    public string text;

    public TresholdFragment(int treshold, string text) {
        this.treshold = treshold;
        this.text = text;
    }

    public override string ToString() {
        return "Treshold = " + treshold + "\n" + text;
    }
}


public class TresholdText {

    public static string NEW_FRAGMENT_SYMBOLE = "###New Fragment###";
    public static string FRAGMENT_TRESHOLD_SYMBOLE = "#FragmentTreshold=";

    protected string path;
    protected List<TresholdFragment> fragments;

    public TresholdText(string path) {
        this.path = path;
        ComputeFragments();
        RevertTresholdOrderFragments();
    }

    protected void ComputeFragments() {
        fragments = new List<TresholdFragment>();
        StreamReader reader = new StreamReader(path);

        string line = reader.ReadLine();
        if(line != NEW_FRAGMENT_SYMBOLE && line != null)
            Debug.LogErrorFormat("Un fragment doit commencer par {0}, ici le fragment vaut \n{1}", NEW_FRAGMENT_SYMBOLE, line);
        while((line = reader.ReadLine()) != null) {
            if(line.StartsWith(FRAGMENT_TRESHOLD_SYMBOLE)) {
                int treshold = int.Parse(line.Split(' ')[1]);
                string content = "";
                line = reader.ReadLine();
                while(line != NEW_FRAGMENT_SYMBOLE && line != null) {
                    content += line + "\n";
                    line = reader.ReadLine();
                }
                fragments.Add(new TresholdFragment(treshold, content));
            }
        }

        reader.Close();
    }

    public List<TresholdFragment> GetAllFragments() {
        return fragments;
    }

    public List<TresholdFragment> GetUnderTresholdFragments(int treshold) {
        List<TresholdFragment> res = new List<TresholdFragment>();
        foreach (TresholdFragment tf in fragments) {
            if(tf.treshold <= treshold)
                res.Add(tf);
        }
        return res;
    }

    public List<TresholdFragment> GetOverTresholdFragments(int treshold) {
        List<TresholdFragment> res = new List<TresholdFragment>();
        foreach (TresholdFragment tf in fragments) {
            if(tf.treshold >= treshold)
                res.Add(tf);
        }
        return res;
    }

    protected static string GetStringFromFragments(List<TresholdFragment> fragments) {
        string res = "";
        foreach (TresholdFragment fragment in fragments)
            res += fragment.text;
        return res;
    }

    public string GetAllFragmentsString() {
        return GetStringFromFragments(GetAllFragments());
    }

    public string GetUnderTresholdFragmentsString(int treshold) {
        return GetStringFromFragments(GetUnderTresholdFragments(treshold));
    }

    public string GetOverTresholdFragmentsString(int treshold) {
        return GetStringFromFragments(GetOverTresholdFragments(treshold));
    }

    protected void RevertTresholdOrderFragments() {
        fragments = fragments.OrderBy(fragment => -fragment.treshold).ToList();
    }

    public List<int> GetAllTresholds() {
        List<int> tresholds = new List<int>();
        foreach(TresholdFragment fragment in fragments) {
            tresholds.Add(fragment.treshold);
        }
        return tresholds;
    }
}
