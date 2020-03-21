using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graphe<T> {

    public Dictionary<T, List<T>> graphe;

    public Graphe(List<T> sommets) {
        graphe = new Dictionary<T, List<T>>();
        foreach(T sommet in sommets) {
            graphe[sommet] = new List<T>();
        }
    }
    public Graphe() {
        graphe = new Dictionary<T, List<T>>();
    }

    public void SetSommet(T sommet, List<T> voisins) {
        graphe[sommet] = voisins;
    }

    // Override []
    public List<T> this[T key] {
        get => GetValue(key);
        set => SetSommet(key, value);
    }

    public List<T> GetValue(T key) {
        return graphe[key];
    }

    public int GetNbSommets() {
        return graphe.Count;
    }

    public List<T> CycleHamiltonien(T depart) {
        List<T> cycle = SolveHamiltonien(
            marqued: new List<T>(),
            depart: depart,
            current: depart);
        if (cycle.Count == GetNbSommets()) {
            return cycle;
        } else {
            throw new Exception("Pas de cycle hamiltonien pour ce graphe !");
        }
    }

    protected List<T> SolveHamiltonien(List<T> marqued, T depart, T current) {
        if(marqued.Count == GetNbSommets() && marqued[marqued.Count - 1].Equals(depart)) {
            return marqued;
        }

        List<T> voisins = this[current];
        foreach(T voisin in voisins) {
            if(!marqued.Contains(voisin)) {
                marqued.Add(voisin);
                List<T> res = SolveHamiltonien(marqued, depart, voisin);
                if(res.Count == GetNbSommets() && res[res.Count - 1].Equals(depart)) {
                    return res;
                }
                marqued.RemoveAt(marqued.Count - 1);
            }
        }

        return new List<T>();
    }
}
