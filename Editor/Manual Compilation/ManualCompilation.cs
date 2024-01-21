#if UNITY_2019_3_OR_NEWER
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityToolbarExtender;
#elif UNITY_2017_1_OR_NEWER
 using System.Reflection;
#endif

namespace Essentials.ManualCompilation

{
    /// <summary>
    /// Force l'arrêt de la compilation automatique d'Unity
    /// et permet de la lancer manuellement.
    /// </summary>
    public sealed class ManualCompilation : AssetPostprocessor
    {
        #region Constantes

        /// <summary>
        /// Les chemins d'accès de chaque icône
        /// </summary>

        public const string RECOMPILE_ICON_PATH = "icon_recompile"/*"Packages/com.clement.essentials.manual-compilation/Editor/Manual Compilation/Resources/icon_recompile.psd"*/;
        public const string RECOMPILE_AND_PLAY_ICON_PATH = "icon_recompile and play"/*"Packages/com.clement.essentials.manual-compilation/Editor/Manual Compilation/Resources/icon_recompile and play.psd"*/;
        public const string REFRESH_ICON_PATH = "icon_refresh"/*"Packages/com.clement.essentials.manual-compilation/Editor/Manual Compilation/Resources/icon_refresh.psd"*/;

        #endregion

        #region Variables d'instance

        /// <summary>
        /// Les images des boutons
        /// </summary>

        private static Texture _recompileIcon;
        private static Texture _recompileAndPlayIcon;
        private static Texture _refreshIcon;

        #endregion

        #region Fonctions publiques

        /// <summary>
        /// Active ou non le rafraîchissement des assets quand la compilation
        /// est manuelle ou non
        /// </summary>
        /// <param name="enableManualCompilation"><see langword="true"/> si la compilation est manuelle, <see langword="false"/> sinon</param>
        public static void SetCompilationState(bool enableManualCompilation)
        {
            if (enableManualCompilation)
            {
                // Empêche la recompilation automatique

                EditorApplication.LockReloadAssemblies();
                AssetDatabase.DisallowAutoRefresh();

                EditorSettings.enterPlayModeOptionsEnabled = true;
                EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            }
            else
            {
                // Autorise à nouveau la compilation auto. et recharge auto. des assets 

                EditorApplication.UnlockReloadAssemblies();
                AssetDatabase.AllowAutoRefresh();
                AssetDatabase.Refresh();

                // Le bouton Play doit à nouveau recharger les domaines

                EditorSettings.enterPlayModeOptionsEnabled = false;
            }
        }

        /// <summary>
        /// Recompile les scripts manuellement
        /// </summary>
        public static void Recompile()
        {
            EditorApplication.UnlockReloadAssemblies();

#if UNITY_2019_3_OR_NEWER
            bool cleanBuildCache = Menu.GetChecked(ManualCompilationMenuItems.CLEAN_BUILD_CACHE_PATH);
            CompilationPipeline.RequestScriptCompilation(cleanBuildCache ? RequestScriptCompilationOptions.CleanBuildCache : RequestScriptCompilationOptions.None);
#elif UNITY_2017_1_OR_NEWER
            var editorAssembly = Assembly.GetAssembly(typeof(Editor));
            var editorCompilationInterfaceType = editorAssembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
            var dirtyAllScriptsMethod = editorCompilationInterfaceType.GetMethod("DirtyAllScripts", BindingFlags.Static | BindingFlags.Public);
            dirtyAllScriptsMethod.Invoke(editorCompilationInterfaceType, null);
#endif
        }

        /// <summary>
        /// Recompile les scripts manuellement
        /// et lance le mode Jeu
        /// </summary>
        public static void RecompileAndPlay()
        {
            EditorSettings.enterPlayModeOptionsEnabled = false;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;

            CompilationPipeline.compilationFinished += OnCompileAndPlayFinished;

            Recompile();
        }

        /// <summary>
        /// Permet de rafraîchir les assets de l'onglet Project
        /// </summary>
        public static void RefreshAssets()
        {
            EditorApplication.UnlockReloadAssemblies();
            AssetDatabase.Refresh();
        }

        #endregion

        #region Fonctions privées

        /// <summary>
        /// S'abonne au ToolbarExtender pour créer des boutons
        /// à côté des boutons du mode Play
        /// </summary>
        private static void OnToolbarGUI()
        {
            bool manualCompilationEnabled = EditorPrefs.GetBool(ManualCompilationMenuItems.ENABLE_MANUAL_COMPILATION_PATH);

            if (!manualCompilationEnabled)
            {
                return;
            }

            GUILayout.FlexibleSpace();

            if (EditorApplication.isCompiling)
            {
                GUI.enabled = false;
            }

            // Relance la compilation manuellement depuis un bouton dans la Toolbar d'Unity

            if (GUILayout.Button(new GUIContent(_refreshIcon, "Refresh Assets"), EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                if (!EditorApplication.isPlaying)
                {
                    RefreshAssets();
                }
            }

            // Relance la compilation manuellement depuis un bouton dans la Toolbar d'Unity

            if (GUILayout.Button(new GUIContent(_recompileIcon, "Recompile"), EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                if (!EditorApplication.isPlaying)
                {
                    Recompile();
                }
            }

            // Recompile et lance le jeu (le bouton Play par défaut ne recompilera pas les scripts)

            if (GUILayout.Button(new GUIContent(_recompileAndPlayIcon, "Recompile And Play"), EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.ExitPlaymode();
                }
                else
                {
                    RecompileAndPlay();
                }
            }
        }

        /// <summary>
        /// Appelée quand les scripts sont recompilés
        /// </summary>
        private static void OnCompileAndPlayFinished(object obj)
        {
            EditorApplication.EnterPlaymode();
            CompilationPipeline.compilationFinished -= OnCompileAndPlayFinished;
        }

        /// <summary>
        /// Appelée quand les scripts sont recompilés
        /// ou quand les assets sont réimportées
        /// </summary>
        /// <param name="importedAssets">Les assets importées</param>
        /// <param name="deletedAssets">Les assets supprimées</param>
        /// <param name="movedAssets">Les assets déplacées</param>
        /// <param name="movedFromAssetPaths">Les emplacements d'origine des assets importées</param>
        /// <param name="didDomainReload">Indique si les scripts ont été recompilés</param>
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            // Charge les icônes

            if (_recompileIcon == null)
            {
                _recompileIcon = Resources.Load<Texture>(RECOMPILE_ICON_PATH);
                _recompileAndPlayIcon = Resources.Load<Texture>(RECOMPILE_AND_PLAY_ICON_PATH);
                _refreshIcon = Resources.Load<Texture>(REFRESH_ICON_PATH);
            }

            if (didDomainReload)
            {
                // Extension pour ajouter des boutons dans la Toolbar d'Unity
                // avant ou après les boutons pour lancer le mode Play

                ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
            }
            else
            {
                // Quand on importe des assets, on débloque la compilation
                // si des scripts sont importés.
                // Ca évite de bloquer la compilation manuelle.

                EditorApplication.UnlockReloadAssemblies();

                // Garde le focus sur l'éditeur

                GUI.FocusWindow(0);
            }
        }

        #endregion
    }
}
