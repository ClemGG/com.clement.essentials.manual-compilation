using System.Collections;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

namespace XDFramework.Editor
{
    /// <summary>
    /// Affiche les menus de la compilation manuelle 
    /// dans la barre d'outils d'Unity
    /// </summary>
    [InitializeOnLoad]
    public sealed class ManualCompilationMenuItems
    {
        #region Constantes

        /// <summary>
        /// Les chemins de chaque onglet de la barre d'outils
        /// </summary>

        public const string ENABLE_MANUAL_COMPILATION_PATH = "Manual Compilation/Settings/Enable Manual Compilation";
        public const string CLEAN_BUILD_CACHE_PATH = "Manual Compilation/Settings/Clean Build Cache (fail safe)";
        public const string RECOMPILE_PATH = "Manual Compilation/Recompile (use if buttons are disabled)";
        public const string RECOMPILE_AND_PLAY_PATH = "Manual Compilation/Recompile and Play (use if buttons are disabled)";
        public const string REFRESH_ASSETS_PATH = "Manual Compilation/Refresh Assets (use if buttons are disabled)";
        public const string RESTART_UNITY_PATH = "Manual Compilation/Restart Unity";

        #endregion

        #region Constructeur

        /// <summary>
        /// Le constructeur par défaut
        /// </summary>
        static ManualCompilationMenuItems()
        {
            // On est obligés de délayer car la classe Menu ne se met à jour qu'après l'initialisation

            EditorCoroutineUtility.StartCoroutineOwnerless(ManualCompilationMenuItems.OnStartCo());
        }

        /// <summary>
        /// Lancé quand la classe est collectée par le GC
        /// </summary>
        ~ManualCompilationMenuItems()
        {
            bool manualCompilationEnabled = EditorPrefs.GetBool(ManualCompilationMenuItems.ENABLE_MANUAL_COMPILATION_PATH);

            if (manualCompilationEnabled)
            {
                ManualCompilation.SetCompilationState(false);
            }
        }

        #endregion

        #region Fonctions privées

        /// <summary>
        /// Appelée quand l'éditeur est ouvert
        /// </summary>
        private static IEnumerator OnStartCo()
        {
            yield return null;

            bool enableManualCompilation = EditorPrefs.GetBool(ManualCompilationMenuItems.ENABLE_MANUAL_COMPILATION_PATH);
            bool clearBuildCache = EditorPrefs.GetBool(ManualCompilationMenuItems.CLEAN_BUILD_CACHE_PATH);
            Menu.SetChecked(ManualCompilationMenuItems.ENABLE_MANUAL_COMPILATION_PATH, enableManualCompilation);
            Menu.SetChecked(ManualCompilationMenuItems.CLEAN_BUILD_CACHE_PATH, clearBuildCache);

            // On ne le lance que si la compilation doit être manuelle
            // car le AllowRefresh utilise un compteur au lieu d'un bool

            if (enableManualCompilation)
            {
                ManualCompilation.SetCompilationState(true);
            }
        }

        /// <summary>
        /// Permet d'activer ou non la recompilation manuelle du projet depuis l'éditeur
        /// </summary>
        [MenuItem(ManualCompilationMenuItems.ENABLE_MANUAL_COMPILATION_PATH)]
        private static void ToggleEnableManualCompilationBtn()
        {
            bool enableManualCompilation = Menu.GetChecked(ManualCompilationMenuItems.ENABLE_MANUAL_COMPILATION_PATH);

            enableManualCompilation = !enableManualCompilation;

            Menu.SetChecked(ManualCompilationMenuItems.ENABLE_MANUAL_COMPILATION_PATH, enableManualCompilation);
            EditorPrefs.SetBool(ManualCompilationMenuItems.ENABLE_MANUAL_COMPILATION_PATH, enableManualCompilation);

            ManualCompilation.SetCompilationState(enableManualCompilation);
        }

        /// <summary>
        /// Permet d'activer ou non la recompilation totale du projet depuis l'éditeur
        /// </summary>
        [MenuItem(ManualCompilationMenuItems.CLEAN_BUILD_CACHE_PATH)]
        private static void ToggleCleanBuildCacheMenuBtn()
        {
            bool clearBuildCache = Menu.GetChecked(ManualCompilationMenuItems.CLEAN_BUILD_CACHE_PATH);
            clearBuildCache = !clearBuildCache;
            Menu.SetChecked(ManualCompilationMenuItems.CLEAN_BUILD_CACHE_PATH, clearBuildCache);
            EditorPrefs.SetBool(ManualCompilationMenuItems.CLEAN_BUILD_CACHE_PATH, clearBuildCache);
        }

        /// <summary>
        /// Permet d'activer ou non la recompilation totale du projet depuis l'éditeur
        /// </summary>
        [MenuItem(ManualCompilationMenuItems.RECOMPILE_PATH)]
        private static void RecompileMenuBtn()
        {
            if (!EditorApplication.isPlaying)
            {
                ManualCompilation.Recompile();
            }
        }

        /// <summary>
        /// Permet d'activer ou non la recompilation totale du projet depuis l'éditeur
        /// avant de lancer le mode Jeu
        /// </summary>
        [MenuItem(ManualCompilationMenuItems.RECOMPILE_AND_PLAY_PATH)]
        private static void RecompileAndPlayMenuBtn()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.ExitPlaymode();
            }
            else
            {
                ManualCompilation.RecompileAndPlay();
            }
        }

        /// <summary>
        /// Permet de rafraîchir les assets de l'onglet Project
        /// </summary>
        [MenuItem(ManualCompilationMenuItems.REFRESH_ASSETS_PATH)]
        private static void RefreshAssetsBtn()
        {
            if (!EditorApplication.isPlaying)
            {
                ManualCompilation.RefreshAssets();
            }
        }

        /// <summary>
        /// Relance Unity si besoin
        /// </summary>
        [MenuItem(ManualCompilationMenuItems.RESTART_UNITY_PATH)]
        private static void ReopenProject()
        {
            EditorApplication.OpenProject(Directory.GetCurrentDirectory());
        }

        #endregion
    }
}