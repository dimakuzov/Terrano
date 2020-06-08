using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.XR;
using UnityEngine.Networking;
using UnityEngine.UI;

public class KuzController : MonoBehaviour
{
    //==== public values
    public Camera userCamera;
    public GameObject sensorPref;
    public string url;


    public Text debugText;
    [Header("Enable for Editor")]
    public bool isEditor;

    
    //==== private values
    Dictionary<string, DeviceData> allDevice;
    private DeviceData[] allSensor;
    private Vector3 geoLocation;

    private Vector3 compassRotate;


    void Start()
    {
        if(!isEditor)
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        else
        {
            geoLocation = new Vector3(55.592312f,159,38.123182f);
        }
        allDevice = new Dictionary<string, DeviceData>();
        Input.location.Start();
        Input.compass.enabled = true;
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended)
            {
                //debugText.text = "TouchPhase.Ended \n";
                //if (EventSystem.current.currentSelectedGameObject != null)
                //{
                    //debugText.text += "Not touching a UI button, moving on.\n";

                    // Test if you are hitting an existing marker
                    RaycastHit hit = new RaycastHit();
                    Ray ray = userCamera.ScreenPointToRay(touch.position);

                    if (Physics.Raycast(ray, out hit))
                    {
                        //debugText.text += "Selected an existing note.\n";

                        GameObject note = hit.transform.gameObject;
                        //debugText.text += "note name: " + note.name + "\n";


                        InputField input = note.GetComponentInChildren<InputField>();
                        input.interactable = true;
                        input.ActivateInputField();
                        //debugText.text += "input.name: " + input.name + "\n";
                        //input.onEndEdit.AddListener(delegate { OnNoteClosed(input); });

                    }
                //}
            }
        }
    }
    
    private void OnNoteClosed(InputField input)
    {
        Debug.Log("No longer editing current note!");

        // Save input text, and set input field as non interactable
        //mCurrNoteInfo.note = input.text;
        input.DeactivateInputField();
        input.interactable = false;
    }


    //================================================================================================
    
    
    #region Buttons

    public void GetMetadata()
    {
        StartCoroutine(GetMetadata(url + "/api/device/DeviceMetadata/GetAll"));
        //StartCoroutine(GetEventByDeviceId(url + "/api/device/DeviceEvent/GetByDeviceId", "terrano030320202219"));
        //StartCoroutine(GetEvent(url + "/api/device/DeviceEvent/GetAll"));
        //StartCoroutine(GetEvent(url + "/api/device/DeviceEvent/GetUser"));
        //StartCoroutine(GetMetadataByDeviceId(url + "/api/device/DeviceMetadata/GetByDeviceId", "terrano030320202219"));
    }
    
    public void GetLocationButton()
    {
        StartCoroutine(GetLocation());
    }

    public void ShowPointsButton()
    {
        ShowObjectOnScene();
    }
    
/*
    public void MakeSenderData()
    {
        //apiEventData = JsonUtility.FromJson<APIEventData>(MakeEventJSON()); // получаем значения сенсоров
        ReadMetadata(MakeJSON()); // получаем местоположение сенсоров
        ReadEventData(MakeEventJSON()); // получаем значения сенсоров
    }
*/

    #endregion

    
    //================================================================================================
    
    
    #region Get And Post Data

    public IEnumerator GetEventByDeviceId(string url, string id)
    {
        Debug.Log("GetEvent(string url, string id): " + url + ", id: " + id);

        string urlWithParams = string.Format("{0}{1}{2}{3}", url, id, 1, 11);
        using (UnityWebRequest www = UnityWebRequest.Get(urlWithParams))
            //using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else if(www.isDone)
            {
                string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
                Debug.Log("json GetEventByDeviceId Result: " + jsonResult);
            }
        }
    }
    
    public IEnumerator GetEvent(string url)
    {
        Debug.Log("GetEvent string url: " + url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else if(www.isDone)
            {
                string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
                Debug.Log("json GetEvent Result: " + jsonResult);
            }
        }
    }
    
    public IEnumerator GetMetadataByDeviceId(string url, string id)
    {
        Debug.Log("GetMetadataByDeviceId(string url): " + url);
        string urlWithParams = string.Format("{0}{1}", url, id);
        using (UnityWebRequest www = UnityWebRequest.Get(urlWithParams))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else if(www.isDone)
            {
                string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
                Debug.Log("json Metadata Result: " + jsonResult);
                //ReadMetadata(jsonResult);
            }
        }
    }
    
    
    public IEnumerator GetMetadata(string url)
    {
        Debug.Log("GetMetadata(string url): " + url);
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else if(www.isDone)
            {
                string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
                Debug.Log("json Metadata Result: " + jsonResult);
                ReadMetadata(jsonResult);
            }
        }
    }
    
    
    

    #endregion
    
    
    //================================================================================================


    #region Reading

    void ReadMetadata(string json)
    {
        json = "{\"locationDatas\":" + json + "}";
        APIMetadata apiMetadata = JsonUtility.FromJson<APIMetadata>(json);
        
        foreach (var locationData in apiMetadata.locationDatas)
        {
            DeviceData deviceData = new DeviceData();
            deviceData.id = locationData.deviceId;
            deviceData.type = locationData.deviceType;
        
            char[] charSeparators = new char[]{','};
            string[] result = locationData.deviceGeolocation.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            deviceData.latitude = float.Parse(result[0]);
            deviceData.longitude = float.Parse(result[1]);
            allDevice.Add(deviceData.id, deviceData);

            //StartCoroutine(GetEventByDeviceId(url + "/api/device/DeviceEvent/GetByDeviceId", deviceData.id));
        }
        //StartCoroutine(GetEvent(url + "/api/device/DeviceEvent/GetAll"));
    }

    void ReadEventData(string json)
    {
        APIEventData apiEventData = JsonUtility.FromJson<APIEventData>(json);

        DeviceData deviceData = allDevice[apiEventData.id];
        deviceData.sensorT1 = float.Parse(apiEventData.events[0].T1);
        deviceData.sensorH1 = float.Parse(apiEventData.events[0].H1);
        deviceData.signal = float.Parse(apiEventData.signal);
        
        // ----- show new note in AR
        Vector3 worldPos = new Vector3(deviceData.latitude, geoLocation.y, deviceData.longitude);
        //ShowObjectOnScene(worldPos, deviceData.id);
    }
    
    #endregion


    //================================================================================================
    

    #region Showing

    //void ShowObjectOnScene(Vector3 worldDevicePosition, string id)
    void ShowObjectOnScene()
    {
        
        //Вставить координаты Жуковского и проверить компас на нем
        //Vector3 positionCoeff = geoLocation - userCamera.transform.position;
        Vector3 devicePos = new Vector3(55.592312f,159,38.123182f);
        Debug.Log("zzzzz worldDevicePosition: " + devicePos);
        Debug.Log("zzzzz geoLocation: " + geoLocation);
        devicePos -= geoLocation;
        devicePos.x *= 100000;
        devicePos.z *= 100000;
        
        GameObject currDevice = Instantiate(sensorPref, new Vector3(0,0,0), Quaternion.identity);
        currDevice.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);    
        
        //===== compass correst
        currDevice.transform.rotation =  Quaternion.Euler(0, -Input.compass.magneticHeading, 0);

        //===== Setting values on device
        DeviceManager deviceManager = currDevice.GetComponent<DeviceManager>();
        deviceManager.root.position = devicePos;
        deviceManager.id = "";
        deviceManager.ShowValue(88.88f, 88.88f, "Test");

        //===== Turn the note to point at the camera
        Vector3 targetPosition = new Vector3(userCamera.transform.position.x,
            userCamera.transform.position.y,
            userCamera.transform.position.z);
        currDevice.transform.LookAt(targetPosition);
        currDevice.transform.Rotate(0f, -180f, 0f);
        
        
        /*
        foreach (var device in allDevice)
        {

            //Vector3 positionCoeff = geoLocation - userCamera.transform.position;
            Vector3 devicePos = new Vector3(device.Value.latitude, geoLocation.y, device.Value.longitude);
            Debug.Log("zzzzz worldDevicePosition: " + devicePos);
            Debug.Log("zzzzz geoLocation: " + geoLocation);
            devicePos -= geoLocation;
            devicePos.x *= 100000;
            devicePos.z *= 100000;
            
            //===== compass correst
            
            
            

            GameObject currDevice = Instantiate(sensorPref, devicePos, Quaternion.identity);
            currDevice.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);

            // ----- Setting values on device
            DeviceManager deviceManager = currDevice.GetComponent<DeviceManager>();
            deviceManager.id = device.Key;
            deviceManager.ShowValue(allDevice[device.Key].sensorH1, allDevice[device.Key].sensorT1, allDevice[device.Key].type);

            // Turn the note to point at the camera
            Vector3 targetPosition = new Vector3(userCamera.transform.position.x,
                userCamera.transform.position.y,
                userCamera.transform.position.z);
            currDevice.transform.LookAt(targetPosition);
            currDevice.transform.Rotate(0f, -180f, 0f);
        }
        */
    }

    IEnumerator GetLocation()
    {
        
        if (!Input.location.isEnabledByUser)
            yield break;
        
        float yAngle = Input.compass.magneticHeading;
        float yCamera = userCamera.transform.eulerAngles.y;
        
        if (yAngle >= 180)
        {
            yAngle = -360 + yAngle;
        }

        if (yCamera >= 180)
        {
            yCamera = -360 + yCamera;
        }
        
        Debug.Log("zzzzz Input.compass.magneticHeading: " + yAngle); //если больше 180 то инвентировать в меньше нуля
        Debug.Log("zzzzz userCamera.transform.eulerAngles.y: " + yCamera); //если больше 180 то инвентировать в меньше нуля
        
        
        
        yAngle -= userCamera.transform.eulerAngles.y;
        Debug.Log("zzzzz yAngle - cameraTransform: " + yAngle);
        //yAngle += userCamera.transform.eulerAngles.y;
        
        compassRotate = new Vector3(0,-Input.compass.magneticHeading, 0);
        
        
        
        
        
        
        
        
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("zzzzz Unable to determine device location");
            yield break;
        }
        else
        {
            //print("Location: " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
            geoLocation.x = Input.location.lastData.latitude;
            geoLocation.y = Input.location.lastData.altitude;
            geoLocation.z = Input.location.lastData.longitude;
        }
        Debug.Log("zzzzz geoLocation: " + geoLocation);

        Input.location.Stop();
        yield return null;
    }
    
    #endregion
    
    
    //================================================================================================
    

    #region Temp (making json)

    string MakeJSON()
    {
        string tempJson = "";
        APILocationData senderData = new APILocationData();
        APISensorData sensorData1;
        APISensorData sensorData2;
        sensorData1 = new APISensorData();
        sensorData1.Type = "SOIL";
        sensorData1.Firmware = "1.02";
        sensorData1.Depth = "20";
        sensorData2 = new APISensorData();
        sensorData2.Type = "SOIL";
        sensorData2.Firmware = "1.02";
        sensorData2.Depth = "40";

        senderData.deviceId = "terrano030320202218";
        senderData.organizationName = "Brisbane City Council";
        senderData.organizationId = "Org-BrisbaneCityCouncil";
        senderData.deviceType = "Soil";
        senderData.deviceNumber = "20000303";
        senderData.deviceGeolocation = "55.592153,38.122425";
        senderData.communication = "CAT-M";
        senderData.simcardNumber = "8500344389875N";
        senderData.mobileOperator = "Telstra";
        senderData.firmwareVersion = "2.02";
        senderData.sensors = new APISensorData[] {sensorData1, sensorData2};
        senderData.id = "62a49b34-19ca-4459-abca-d088eb3f557e";
        
        APIMetadata apiMetadata = new APIMetadata();
        apiMetadata.locationDatas = new APILocationData[] {senderData, senderData};
        return JsonUtility.ToJson(apiMetadata);
        return JsonUtility.ToJson(senderData);
    }

    string MakeEventJSON()
    {
        APIEventData deviceSensors = new APIEventData();
        APIDeviceEvents deviceEvents = new APIDeviceEvents();
        deviceEvents.H1 = "54.13696294843078";
        deviceEvents.T1 = "32.85748434386099";
        deviceSensors.deviceId = "terrano030320202218";
        deviceSensors.signal = "95";
        deviceSensors.events = new APIDeviceEvents[]{deviceEvents};
        
        return JsonUtility.ToJson(deviceSensors);
    }
    
    #endregion
    
}


//================================================================================================
//==========================================  Classes  ===========================================
//================================================================================================



[SerializeField]
class DeviceData
{
    public string id;
    public string type;
    public string firmwareVersion;
    public int sensorsCount;
    
    public float latitude;
    public float longitude;
    public float altitude;

    public float sensorT1;
    public float sensorH1;
    public float signal;
}

// =========================== API Metadata ===============================


[Serializable]
public class APIMetadata
{
    public APILocationData[] locationDatas;
}

[Serializable]
public class APILocationData
{
    public string deviceId;// = "terrano030320202218";
    public string organizationName;// = "Brisbane City Council";
    public string organizationId;// = "Org-BrisbaneCityCouncil";
    public string deviceType;// = "Soil";
    public string deviceNumber;// = "20000303";
    public string deviceGeolocation;// = "-27.481423, 153.045056";
    public string communication;// = "CAT-M";
    public string simcardNumber;// = "8500344389875N";
    public string mobileOperator;// = "Telstra";
    public string firmwareVersion;// = "2.02";
    public APISensorData[] sensors;
    public string id;// = "62a49b34-19ca-4459-abca-d088eb3f557e";
}

[Serializable]
public class APISensorData
{
    public string Type;
    public string Firmware;
    public string Depth;
}

// =========================== API Events ===============================

[Serializable]
class APIEventData
{
    public string deviceId;
    public string partitionKey;
    public string signal;
    public string tls_time;
    public string timestamp;
    public APIDeviceEvents[] events;
    public string id;
    public string _rid;
    public string _self;
    public string _etag;
    public string _attachments;
    public string _ts;
}

[Serializable]
public class APIDeviceEvents
{
    public string T1;
    public string H1;
}
