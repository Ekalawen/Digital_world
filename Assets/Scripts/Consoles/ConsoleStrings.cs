using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "ConsoleStrings", menuName = "ConsoleStrings")]
public class ConsoleStrings : ScriptableObject
{

    public LocalizedString initialisationNiveau;
    public LocalizedString initialisationMatrice;
    public LocalizedString vaAussiLoinQueTuPeux;

    public LocalizedString nbLumieresTrouvees;
    public LocalizedString nbEnnemisTrouvees;

    public LocalizedString pouvoirsDesactives;
    public LocalizedString geolocaliserData;
    public LocalizedString apparitionDesDatas;

    public LocalizedString jeSaisOuVousEtes;
    public LocalizedString onLesASemes;

    public LocalizedString toucheParUneSondeImportant;
    public LocalizedString toucheParUneSondeConsole;
    public LocalizedString toucheParUnTracerImportant;
    public LocalizedString toucheParUnTracerConsole;
    public LocalizedString blasteParUnTracerImportant;
    public LocalizedString blasteParUnTracerConsole;
    public LocalizedString blasteParUnTracerImportantLearning;

    public LocalizedString winGameImportant;
    public LocalizedString winGameConsole1;
    public LocalizedString winGameConsole2;
    public LocalizedString winRecompense;
    public LocalizedString loseRecompense;

    public LocalizedString deathTimeOut1;
    public LocalizedString deathTimeOut2;
    public LocalizedString deathFallOut1;
    public LocalizedString deathFallOut2;
    public LocalizedString deathCaptured1;
    public LocalizedString deathCaptured2;
    public LocalizedString deathTouchedDeathCube1;
    public LocalizedString deathTouchedDeathCube2;
    public LocalizedString deathTouchedBouncyCube1;
    public LocalizedString deathTouchedBouncyCube2;
    public LocalizedString deathPouvoirCost1;
    public LocalizedString deathPouvoirCost2;
    public LocalizedString deathOutOfBlocks1;
    public LocalizedString deathOutOfBlocks2;
    public LocalizedString deathFailedJumpEvent1;
    public LocalizedString deathFailedJumpEvent2;
    public LocalizedString deathSondeHit1;
    public LocalizedString deathSondeHit2;
    public LocalizedString deathTracerHit1;
    public LocalizedString deathTracerHit2;
    public LocalizedString deathTracerBlast1;
    public LocalizedString deathTracerBlast2;
    public LocalizedString deathFlirdHit1;
    public LocalizedString deathFlirdHit2;
    public LocalizedString deathFirstBossHit1;
    public LocalizedString deathFirstBossHit2;
    public LocalizedString deathFirstBossBlast1;
    public LocalizedString deathFirstBossBlast2;
    public LocalizedString deathSoulRobberAspiration1;
    public LocalizedString deathSoulRobberAspiration2;
    public LocalizedString deathSecondBossLaser1;
    public LocalizedString deathSecondBossLaser2;

    public LocalizedString plusQueX;
    public LocalizedString localisationIlTeResteXData;
    public LocalizedString localisationIlTeResteXItems;
    public LocalizedString localisationRienTrouve;
    public LocalizedString runDetection;
    public List<LocalizedString> summarizePathfinder;
    public LocalizedString failLocalisationUnauthorized;
    public LocalizedString failLocalisationObjectifInateignable;
    public LocalizedString failLocalisationInEndEvent;

    public LocalizedString wowQuelSaut;
    public LocalizedString bridgeBuilderInvalid;
    public LocalizedString fautTrouverLaSortie;
    public LocalizedString externalStartEndEvent;
    public LocalizedString ilResteQuelqueChoseAFaire;
    public LocalizedString altitudeCritique;

    public LocalizedString changementDeGravite;
    public LocalizedString changementDeGraviteDirection;

    public LocalizedString blackout;

    public LocalizedString tracersAlertes;

    public LocalizedString analyzeLevelDeuxiemeSalveIntrusion;
    public LocalizedString analyzeLevelDeuxiemeSalveReplication;

    public LocalizedString attentionANePasEtreEjecte;
    public LocalizedString attentionAuxCubesDeLaMort;

    public LocalizedString doubleSautActive;
    public LocalizedString doubleSautPlusUn;
    public LocalizedString doubleSautExplications;

    public LocalizedString volActive;
    public LocalizedString volExplications;

    public LocalizedString pouvoirGiverActive;
    public LocalizedString pouvoirGiverExplications;
    public LocalizedString pouvoirGiverDash333SubPhrase;

    public LocalizedString keyA;
    public LocalizedString keyE;
    public LocalizedString keyClicGauche;
    public LocalizedString keyClicDroit;

    public LocalizedString bossChangementDePhaseChargement;
    public LocalizedString bossChangementDePhaseTermine;

    public LocalizedString meilleurScore;

    public LocalizedString autoDestructionEnclenchee;
    public LocalizedString deconnexionEnclenchee;

    public LocalizedString ZQSDinsteadOfArrows;

    public LocalizedString jumpStunImportant;
    public LocalizedString jumpStunConsole;
    public LocalizedString jumpActivation;

    public LocalizedString detecte;
    public LocalizedString dissimule;

    public LocalizedString jumpTimer;

    public LocalizedString pressShiftImportant;
    public LocalizedString pressShiftConsole;

    public LocalizedString analyzeTrapWait;
    public LocalizedString analyzeTrapSortie;

    public LocalizedString lumiereProtectionHacked;

    public LocalizedString dataCount;
    public LocalizedString nbWinsCount;

    public LocalizedString rewardNewRegularTreshold;
    public LocalizedString rewardNewInfinitereshold;
    public LocalizedString rewardInfiniteModeReached;

    public LocalizedString timeResetImportant;
    public LocalizedString timeResetConsole;

    public LocalizedString divideTimeBy2;

    public LocalizedString controllerPlugIn;
    public LocalizedString controllerPlugOut;
    public LocalizedString swapToController;
    public LocalizedString swapToKeyboard;
    public LocalizedString choseAControllerPopupTitle;
    public LocalizedString choseAControllerPopupTexte;

    public LocalizedString restartButtonRestart;
    public LocalizedString restartButtonContinue;

    public LocalizedString setInvincible;
    public LocalizedString unsetInvincible;

    public LocalizedString disableEnnemis;
    public LocalizedString enableEnnemis;

    public LocalizedString setPouvoirsCooldownZero;
    public LocalizedString unsetPouvoirsCooldownZero;

    public LocalizedString initializeOverride;
}
