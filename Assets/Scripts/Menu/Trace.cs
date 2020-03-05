using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trace {

    public static string GenerateTrace() {
        string trace = "";
        trace += GetRandomBit();
        trace += GetRandomBit();
        trace += GetRandomHexadecimal();
        trace += GetRandomHexadecimal();
        return trace;
    }

    public static char GetRandomMajLetter() {
        int start = 65; // A
        int end = 90; // Z
        return (char)Random.Range(start, end + 1);
    }

    public static char GetRandomLowerLetter() {
        int start = 97; // a
        int end = 122; // z
        return (char)Random.Range(start, end + 1);
    }

    public static char GetRandomBit() {
        return (Random.Range(0, 2) == 0) ? '0' : '1';
    }

    public static char GetRandomDigit() {
        int start = 48; // 0
        int end = 57; // 9
        return (char)Random.Range(start, end + 1);
    }

    public static char GetRandomHexadecimal() {
        int startDigits = 48; // 0
        //int endDigits = 57; // 9
        int startLetters = 65; // A
        //int endLetters = 70; // F
        int value = Random.Range(0, 16);
        return (char)((value <= 9) ? startDigits + value : startLetters + value - 10);
    }
}
