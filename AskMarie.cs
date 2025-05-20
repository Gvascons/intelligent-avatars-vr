
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;

public class AskMarie : MonoBehaviour
{
    public string serverUrl = "http://localhost:5000/speech-to-text-and-respond";
    public AudioSource audioSource;

    private InputAction recordAction;
    private AudioClip recordedClip;
    private bool isRecording = false;

    void Awake()
    {
        recordAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/r");
    }

    void OnEnable()
    {
        recordAction.Enable();
    }

    void OnDisable()
    {
        recordAction.Disable();
    }

    void Update()
    {
        if (recordAction.WasPressedThisFrame())
        {
            if (!isRecording)
                StartRecording();
            else
                StopRecordingAndSend();
        }
    }

    void StartRecording()
    {
        recordedClip = Microphone.Start(null, false, 5, 44100);
        isRecording = true;
        UnityEngine.Debug.Log("🎙️ Gravando... Pressione R novamente para parar.");
    }

    void StopRecordingAndSend()
    {
        Microphone.End(null);
        isRecording = false;
        UnityEngine.Debug.Log("🛑 Gravação encerrada. Enviando para o servidor...");

        string path = Path.Combine(UnityEngine.Application.persistentDataPath, "input.wav");
        SaveWav(path, recordedClip);

        byte[] audioData = File.ReadAllBytes(path);
        StartCoroutine(SendAudioToServer(audioData));
    }

    IEnumerator SendAudioToServer(byte[] audioBytes)
    {
        string url = serverUrl;

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioBytes, "audio.wav", "audio/wav");

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.LogError("Erro na requisição: " + request.error);
        }
        else
        {
            UnityEngine.Debug.Log("Resposta do servidor: " + request.downloadHandler.text);

            string json = request.downloadHandler.text;
            string audioPath = ExtractAudioPath(json);
            string fullAudioUrl = "http://localhost:5000" + audioPath;
            StartCoroutine(LoadAudioClip(fullAudioUrl));
        }
    }

    string ExtractAudioPath(string json)
    {
        int startIndex = json.IndexOf("audio_path":"") + 13;
        int endIndex = json.IndexOf(""", startIndex);
        return json.Substring(startIndex, endIndex - startIndex);
    }

    IEnumerator LoadAudioClip(string url)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.LogError("Erro ao carregar áudio: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                UnityEngine.Debug.Log("Tamanho do áudio: " + clip.length + " segundos");
                audioSource.clip = clip;
                audioSource.Play();

                if (audioSource.isPlaying)
                    UnityEngine.Debug.Log("🎵 Áudio está tocando!");
                else
                    UnityEngine.Debug.LogWarning("⚠️ Áudio NÃO está tocando!");
            }
        }
    }

    public static void SaveWav(string path, AudioClip clip)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using (var fileStream = CreateEmpty(path))
        {
            ConvertAndWrite(fileStream, clip);
            WriteHeader(fileStream, clip);
        }
    }

    static FileStream CreateEmpty(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();
        for (int i = 0; i < 44; i++) fileStream.WriteByte(emptyByte);
        return fileStream;
    }

    static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    {
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);
        Int16[] intData = new Int16[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];
        const float rescaleFactor = 32767;

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    static void WriteHeader(FileStream fileStream, AudioClip clip)
    {
        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = clip.samples;

        fileStream.Seek(0, SeekOrigin.Begin);
        fileStream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        fileStream.Write(BitConverter.GetBytes(fileStream.Length - 8), 0, 4);
        fileStream.Write(System.Text.Encoding.UTF8.GetBytes("WAVEfmt "), 0, 8);
        fileStream.Write(BitConverter.GetBytes(16), 0, 4);
        fileStream.Write(BitConverter.GetBytes((ushort)1), 0, 2);
        fileStream.Write(BitConverter.GetBytes(channels), 0, 2);
        fileStream.Write(BitConverter.GetBytes(hz), 0, 4);
        fileStream.Write(BitConverter.GetBytes(hz * channels * 2), 0, 4);
        fileStream.Write(BitConverter.GetBytes((ushort)(channels * 2)), 0, 2);
        fileStream.Write(BitConverter.GetBytes((ushort)16), 0, 2);
        fileStream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        fileStream.Write(BitConverter.GetBytes(samples * channels * 2), 0, 4);
    }
}
