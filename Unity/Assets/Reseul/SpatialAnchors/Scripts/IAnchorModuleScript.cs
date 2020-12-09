// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Com.Reseul.SpatialAnchors
{
    /// <summary>
    ///     Azure Spatial Anchors�̏��������s���邽�߂̊Ǘ��N���X�Ɏ������C���^�[�t�F�[�X
    /// </summary>
    public interface IAnchorModuleScript
    {
        /// <summary>
        ///     �A���J�[�擾��Ɏ��s����ʏ��������R���g���[���N���X
        /// </summary>
        IASACallBackManager CallBackManager { set; get; }

        /// <summary>
        /// CoarseRelocation�@�\���L������GPS�Z���T�[�̏�Ԃ��擾���܂��B
        /// </summary>
        SensorStatus GeoLocationStatus { get; }

        /// <summary>
        /// CoarseRelocation�@�\���L������Wifi�̏�Ԃ��擾���܂��B
        /// </summary>
        SensorStatus WifiStatus { get; }

        /// <summary>
        /// CoarseRelocation�@�\���L������Bluetooth�̏�Ԃ��擾���܂��B
        /// </summary>
        SensorStatus BluetoothStatus { get; }

        /// <summary>
        /// �o�^�ς݂̃Z���T�[���̈ꗗ���擾���܂��B
        /// </summary>
        string[] KnownBeaconProximityUuids { get; }

        /// <summary>
        ///     ���������������{���܂�
        /// </summary>
        void Start();

        /// <summary>
        ///     �t���[�����Ɏ��s���鏈�������{���܂��B
        /// </summary>
        void Update();

        /// <summary>
        ///     �I�u�W�F�N�g�̌㏈���i�p���j�����{���܂��B
        /// </summary>
        void OnDestroy();

        /// <summary>
        ///     Azure Spatial Anchors�T�[�r�X�Ƃ̐ڑ����s���A�Z�V�������J�n���܂��B
        /// </summary>
        /// <returns></returns>
        Task StartAzureSessionAsync();

        /// <summary>
        ///     Azure Spatial Anchors�T�[�r�X�Ƃ̐ڑ����~���܂��B
        /// </summary>
        /// <returns></returns>
        Task StopAzureSessionAsync();

        /// <summary>
        ///     Azure Spatial Anchors����擾�ς݂�Spatial Anchor��AppProperties��ύX���܂��B
        ///     �L�[�����łɑ��݂���ꍇ��replace�p�����[�^�̒l�ɉ����Ēu�����A�ǋL��؂�ւ��ď��������{���܂��B
        /// </summary>
        /// <param name="anchorId">Anchor Id</param>
        /// <param name="key">AppProperties�̃L�[</param>
        /// <param name="val">�L�[�ɑΉ�����l</param>
        /// <param name="replace">true:�㏑���Afalse:�J���}��؂�ŒǋL</param>
        void UpdatePropertiesAsync(string anchorId, string key, string val, bool replace = true);

        /// <summary>
        ///     Azure Spatial Anchors����擾�ς݂�Spatial Anchor��AppProperties���ꊇ�ŕύX���܂��B
        ///     �L�[�����łɑ��݂���ꍇ��replace�p�����[�^�̒l�ɉ����Ēu�����A�ǋL��؂�ւ��ď��������{���܂��B
        /// </summary>
        /// <param name="key">AppProperties�̃L�[</param>
        /// <param name="val">�L�[�ɑΉ�����l</param>
        /// <param name="replace">true:�㏑���Afalse:�J���}��؂�ŒǋL</param>
        void UpdatePropertiesAllAsync(string key, string val, bool replace = true);

        /// <summary>
        ///     Spatial Anchor�̌����͈͂�ݒ肵�܂��B
        /// </summary>
        /// <param name="distanceInMeters">�����͈́i�P��:m�j</param>
        void SetDistanceInMeters(float distanceInMeters);

        /// <summary>
        ///     Spatial Anchor�̓�����������ݒ肵�܂��B
        /// </summary>
        /// <param name="distanceInMeters">������</param>
        void SetMaxResultCount(int maxResultCount);

        /// <summary>
        ///     Spatial Anchor�̎�����ݒ肵�܂�
        /// </summary>
        /// <param name="expiration">Anchor�̓o�^���ԁi�P��:���j</param>
        void SetExpiration(int expiration);

        /// <summary>
        ///     Azure Spatial Anchors�T�[�r�X�ɃA���J�[��ǉ����܂��B
        /// </summary>
        /// <param name="theObject">Spatial Anchor�̏��Ƃ��ēo�^���錻����Ԃɐݒu�����I�u�W�F�N�g</param>
        /// <param name="appProperties">Spatial Anchor�Ɋ܂߂���</param>
        /// <returns>�o�^����AnchorId</returns>
        Task<string> CreateAzureAnchorAsync(GameObject theObject, IDictionary<string, string> appProperties);

        /// <summary>
        ///     �w�肳�ꂽSensor���𗘗p�����ӂɃA���J�[�����݂��邩���������{���܂��B
        /// </summary>
        void FindNearBySensors();

        /// <summary>
        ///     �w�肳�ꂽAnchorId�œo�^���ꂽAnchor�𒆐S�ɑ��̃A���J�[�����݂��邩���������{���܂��B
        /// </summary>
        /// <param name="anchorId">��ɂȂ�AnchorId</param>
        void FindNearByAnchor(string anchorId);

        /// <summary>
        ///     �w�肳�ꂽAnchorId�œo�^���ꂽAnchor�𒆐S�ɑ��̃A���J�[�����݂��邩���������{���܂��B
        /// </summary>
        /// <param name="anchorId">��ɂȂ�AnchorId</param>
        void FindAzureAnchorById(params string[] azureAnchorIds);

        /// <summary>
        ///     Azure Spatial Anchors�T�[�r�X����擾�ς݂̃A���J�[����w��̃A���J�[�̂ݍ폜���܂��B
        /// </summary>
        Task DeleteAzureAnchorAsync(string anchorId);

        /// <summary>
        ///     Azure Spatial Anchors�T�[�r�X����擾�ς݂̂��ׂẴA���J�[���폜���܂��B
        /// </summary>
        void DeleteAllAzureAnchorAsync();

        /// <summary>
        ///     �����󋵂��o�͂���C�x���g
        /// </summary>
        event AnchorModuleProxy.FeedbackDescription OnFeedbackDescription;

        /// <summary>
        ///     Anchor�������������s���邽�߂̃R���g���[����ݒ肵�܂��B
        /// </summary>
        /// <param name="iasaCallBackManager"></param>
        void SetASACallBackManager(IASACallBackManager iasaCallBackManager);

        /// <summary>
        /// Coarse Relocation(Bluetooth)�̗L��/������ݒ肵�܂��B
        /// </summary>
        /// <param name="enabledBluetooth">Bluetooth�ɂ��Coarse Relocation�̗L��/����</param>
        /// <param name="knownBeaconProximityUuids">���p����r�[�R����UUID���X�g</param>
        void SetCoarseRelocationBluetooth(bool enabledBluetooth, string[] knownBeaconProximityUuids = null);

        /// <summary>
        /// Coarse Relocation(Wifi)�̗L��/������ݒ肵�܂��B
        /// </summary>
        /// <param name="enabledWifi">Wifi�ɂ��Coarse Relocation�̗L��/����</param>
        void SetCoarseRelocationWifi(bool enabledWifi);

        /// <summary>
        /// Coarse Relocation(GPS)�̗L��/������ݒ肵�܂��B
        /// </summary>
        /// <param name="enabledGeoLocation">GPS�ɂ��Coarse Relocation�̗L��/����</param>
        void SetCoarseRelocationGeoLocation(bool enabledGeoLocation);

        /// <summary>
        /// Spatial Anchors�ŗ��p���郂�[�h��ݒ肵�܂��B
        /// </summary>
        /// <param name="mode"><see cref="ASAMode"/></param>
        /// <returns></returns>
        Task<bool> SetSpatialAnchorModeAsync(ASAMode mode);

        /// <summary> 
        /// Spatial Anchors�ŗ��p���郂�[�h��ݒ肵�܂��B
        /// </summary>
        /// <param name="mode"><see cref="ASAMode"/></param>
        void SetSpatialAnchorMode(ASAMode mode);

        /// <summary>
        /// �w��̃A���J�[�����폜���܂��B
        /// </summary>
        /// <param name="anchorGameObject"></param>
        void DeleteNativeAnchor(GameObject anchorGameObject);

        /// <summary>
        /// Spatial Anchor�o�^���ɕK�v�ȋ�ԏ��̎��W�����擾���܂��B
        /// </summary>
        float RecommendedForCreateProgress { get; }
    }
}