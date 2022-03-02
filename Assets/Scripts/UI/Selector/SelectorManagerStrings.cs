using System;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "SelectorManagerStrings", menuName = "SelectorManagerStrings")]
public class SelectorManagerStrings : ScriptableObject {

    public LocalizedString pathGoodUnlockedTitle;
    public LocalizedString pathGoodUnlockedTexte;
    public LocalizedString pathGoodLockedTitle;
    public LocalizedString pathGoodLockedTexte;
    public LocalizedString pathFalseLockedTitle;
    public LocalizedString pathFalseLockedTexte;
    public LocalizedString pathFalseLockedTexteWithoutAdvice;
    public LocalizedString pathFalseUnlockedTitre;
    public LocalizedString pathFalseUnlockedTexte;

    public LocalizedString blocs;
    public LocalizedString victoires;
    public LocalizedString victoiresZero;
    public LocalizedString plusieursVictoires;

    public LocalizedString archivesTitle;
    public LocalizedString docTitle;
    public LocalizedString dataHackees;
    public LocalizedString dataHackeesJamaisHackeesTexte;

    public LocalizedString palierTotalTexte;
    public LocalizedString palierNextPalierDernier;
    public LocalizedString palierNextPalierProchain;
    public LocalizedString palierNotUnlockedYet;
    public LocalizedString palierCurrentTresholdRegular;
    public LocalizedString palierCurrentTresholdInfinite;
    public LocalizedString palierCurrentTresholdNbVictories;

    public LocalizedString traceDisplayer;

    public LocalizedString meilleurScoreTitre;
    public LocalizedString meilleurScoreTexte;

    public LocalizedString palierDebloqueTitre;
    public LocalizedString palierDebloqueTexte;
    public LocalizedString palierDebloqueUnPalier;

    public LocalizedString niveauDebloqueTitre;
    public LocalizedString niveauDebloqueTexte;

    public LocalizedString niveauVerouilleTitre;
    public LocalizedString niveauVerouilleTotalTexte;
    public LocalizedString niveauVerouilleUnSeulNiveau;
    public LocalizedString niveauVerouilleUnNiveauParmiPlusieurs;

    public LocalizedString tutoriel;
    public LocalizedString tutorielRecommandeTitre;
    public LocalizedString tutorielRecommandeTexte;

    public LocalizedString passe;
    public LocalizedString passes;
    public LocalizedString trace;
    public LocalizedString traces;

    public LocalizedString endBetaTitre;
    public LocalizedTextAsset endBetaTexte;
    public LocalizedString pathDemoDeniedTitre;
    public LocalizedTextAsset pathDemoDeniedTexte;
    public LocalizedString buttonWishlistOnSteam;
    public LocalizedString buttonWishlistOnSteamTooltip;

    public LocalizedString controllerPlugInTitle;
    public LocalizedString controllerPlugInTexte;
    public LocalizedString controllerPlugOutTitle;
    public LocalizedString controllerPlugOutTexte;
}
