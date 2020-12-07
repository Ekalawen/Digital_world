using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

    public void ApplyReplacementEvaluator(Tuple<string, MatchEvaluator> replacement) {
        string source = replacement.Item1;
        MatchEvaluator evaluator = replacement.Item2;
        text = Regex.Replace(text, source, evaluator);
    }
}


public class TresholdText {

    public static string NEW_FRAGMENT_SYMBOLE = "###New Fragment###";
    public static string FRAGMENT_TRESHOLD_SYMBOLE = "#FragmentTreshold=";

    protected TextAsset textAsset;
    protected string content;
    protected int textAssetLineIndice = 0;
    protected List<TresholdFragment> fragments;

    public TresholdText(string content) {
        this.textAsset = null;
        this.content = content;
        ComputeFragments();
        //RevertTresholdOrderFragments();
    }

    public TresholdText(TextAsset textAsset) {
        this.textAsset = textAsset;
        this.content = textAsset.text;
        ComputeFragments();
        //RevertTresholdOrderFragments();
    }

    protected string ReadLine() {
        string[] splited = content.Split('\n');
        if (textAssetLineIndice >= splited.Length)
            return null;
        string res = splited[textAssetLineIndice].Trim();
        textAssetLineIndice++;
        return res;
    }

    protected void ComputeFragments() {
        fragments = new List<TresholdFragment>();
        textAssetLineIndice = 0;

        string line = ReadLine();
        if(line != NEW_FRAGMENT_SYMBOLE && line != null)
            Debug.LogErrorFormat("Un fragment doit commencer par {0}, ici le fragment vaut \n{1}", NEW_FRAGMENT_SYMBOLE, line);
        while((line = ReadLine()) != null) {
            if(line.StartsWith(FRAGMENT_TRESHOLD_SYMBOLE)) {
                int treshold = int.Parse(line.Split(' ')[1]);
                string content = "";
                line = ReadLine();
                while(line != NEW_FRAGMENT_SYMBOLE && line != null) {
                    content += line + "\n";
                    line = ReadLine();
                }
                fragments.Add(new TresholdFragment(treshold, content));
            }
        }
    }

    public List<TresholdFragment> GetAllFragments() {
        return fragments;
    }

    public List<TresholdFragment> GetAllFragmentsOrdered() {
        return fragments.OrderBy((TresholdFragment fragment) => fragment.treshold).ToList();
    }

    public TresholdFragment GetFirstFragment() {
        return GetAllFragmentsOrdered().First();
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
        return GetAllFragmentsOrdered().Select(f => f.treshold).ToList();
    }

    public void ApplyReplacementEvaluatorToAllFragment(Tuple<string, MatchEvaluator> replacement) {
        for(int i = 0; i < fragments.Count; i++) {
            fragments[i].ApplyReplacementEvaluator(replacement);
        }
    }

    public TresholdFragment GetLastFragment() {
        return GetAllFragmentsOrdered().Last();
    }
}
