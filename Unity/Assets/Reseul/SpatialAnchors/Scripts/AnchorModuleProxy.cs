// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using UnityEngine;

namespace Com.Reseul.SpatialAnchors
{
    /// <summary>
    ///     Azure Spatial Anchors�̃T�[�r�X�ւ̃A�N�Z�X���s���v���L�V�N���X
    /// </summary>
    /// <remarks>
    ///     Azure Spatial Anchors��Unity Editor��ł̓G���[�œ��삵�Ȃ����߁AUnity Editor��̓X�^�u�œ��삷��悤�ɂ��̃N���X��񋟂��Ă��܂��B
    /// </remarks>
    public class AnchorModuleProxy : MonoBehaviour
    {

        private static IAnchorModuleScript script;

        private static object lockObj = new object();

        /// <summary>
        ///     �����̓r���o�߂�\�����邽�߂̃��O�o�͗p�f���Q�[�g�B�ʓr�A<see cref="AnchorFeedbackScript" />���Ōďo���܂��B
        /// </summary>
        /// <param name="description">���b�Z�[�W���e</param>
        /// <param name="isOverWrite">���O�̃��b�Z�[�W���㏑�����邩�ǂ����BTrue�̏ꍇ�͏㏑������</param>
        /// <param name="isReset">���O�̃��b�Z�[�W���폜���邩�ǂ����BTrue�̏ꍇ�͂���܂ł̏o�͂��폜���Ă���\������</param>
        public delegate void FeedbackDescription(string description, bool isOverWrite = false, bool isReset = false);

        public float DistanceInMeters => distanceInMeters;

    #region Static Methods

        /// <summary>
        ///     Azure Spatial Anchors�̏��������s����N���X�̃C���X�^���X���擾���܂��B
        /// </summary>
        public static IAnchorModuleScript Instance
        {
            get
            {
                if (script == null)
                {
                    lock (lockObj)
                    {
                        if (script == null)
                        {
#if UNITY_EDITOR
                            var module = FindObjectsOfType<AnchorModuleScriptForStub>();
#else
                var module = FindObjectsOfType<AnchorModuleScript>();
#endif
                            if (module.Length == 1)
                            {
                                var proxy = FindObjectOfType<AnchorModuleProxy>();
                                //Azure Spatial Anchors �ŗ��p����p�����[�^��ݒ肵�܂��B
                                module[0].SetDistanceInMeters(proxy.distanceInMeters);
                                module[0].SetMaxResultCount(proxy.maxResultCount);
                                module[0].SetExpiration(proxy.expiration);
                                module[0].SetSpatialAnchorMode(proxy.defaultMode);
                                module[0].SetCoarseRelocationWifi(proxy.enabledWifi);
                                module[0].SetCoarseRelocationGeoLocation(proxy.enabledGeolocation);
                                module[0].SetCoarseRelocationBluetooth(proxy.enabledBluetooth, proxy.knownBeaconProximityUuids);
                                script = module[0];
                            }
                            else
                            {
                                Debug.LogWarning(
                                    "Not found an existing AnchorModuleScript in your scene. The Anchor Module Script requires only one.");
                            }
                        }
                    }
                }
                return script;
            }
        }

    #endregion

    #region Unity Lifecycle

        private void Start()
        {
#if UNITY_EDITOR
            // Unity Editor���s����Azure Spatial Anchors�{�̂̃I�u�W�F�N�g�𖳌������܂��B
            transform.GetChild(0).gameObject.SetActive(false);
#endif
        }

    #endregion

    #region Inspector Properites

        [Header("NearbySetting")]
        [SerializeField]
        [Tooltip("Maximum distance in meters from the source anchor (defaults to 5).")]
        private float distanceInMeters = 5f;

        [SerializeField]
        [Tooltip("Maximum desired result count (defaults to 20).")]
        private int maxResultCount = 20;

        [Header("CreateAnchorParams")]
        [SerializeField]
        [Tooltip("The number of days until the anchor is automatically deleted")]
        private int expiration = 7;

        [Header("AnchorOperationMode")]
        [SerializeField]
        [Tooltip("Initial Azure Spatial Anchors mode")]
        private ASAMode defaultMode = ASAMode.ById;

        [SerializeField]
        [Tooltip("Using Coarse Relocation,bluetooth is enabled")]
        private bool enabledBluetooth = false;

        [SerializeField]
        [Tooltip("Using bluetooth,set Uuids of beacon")]
        private string[] knownBeaconProximityUuids = new string[]{};

        [SerializeField]
        [Tooltip("Using Coarse Relocation,wifi is enabled")]
        private bool enabledWifi = false;

        [SerializeField]
        [Tooltip("Using Coarse Relocation,geo Location is enabled")]
        private bool enabledGeolocation = false;



        #endregion
    }
}