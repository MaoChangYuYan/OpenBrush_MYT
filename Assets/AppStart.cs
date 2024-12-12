using UnityEngine;
using System;
using System.Security.Cryptography;
using System.Text;

public class AppStart : MonoBehaviour
{
    private const string firstRunKey = "FirstRunTimeEncrypted_1212"; // ���ܴ洢���״�����ʱ��
    private const int expirationTime = 5 * 24 * 60 * 60; // ����Ϊ7����ڣ���λ���룩
    private const string LastValidTime = "_lastValidTime_1212";
    private DateTime _lastValidTime;//ÿ����������У��ʱ��

    void Start()
    {
        if (!PlayerPrefs.HasKey(firstRunKey)) // �ж��Ƿ��ǵ�һ������
        {
            long firstRunTime = DateTime.UtcNow.Ticks;
            string encryptedTime = Encrypt(firstRunTime.ToString());
            PlayerPrefs.SetString(firstRunKey, encryptedTime);
            PlayerPrefs.SetString(LastValidTime, DateTime.UtcNow.ToString());
            PlayerPrefs.Save();
        }
        else
        {
            string encryptedTime = PlayerPrefs.GetString(firstRunKey);
            string decryptedTime = Decrypt(encryptedTime);
            long firstRunTime = long.Parse(decryptedTime);
            long currentTime = DateTime.UtcNow.Ticks;
            long elapsedTime = (currentTime - firstRunTime) / TimeSpan.TicksPerSecond;

            ValidateTime();

            Debug.LogError("elapsedTime:" + elapsedTime + "   " + expirationTime + "--Validatestate:" + Validatestate + "");

            if (elapsedTime < 0)
            {
                Debug.Log("ʱ���쳣�˳�");
                Application.Quit(); // ����30����˳�Ӧ��
            }

            if (elapsedTime > expirationTime || !Validatestate) // �ж��Ƿ񳬹�30������޸�ϵͳʱ��
            {
                Debug.Log("����ʱ������˳�");
                Application.Quit(); // ����30����˳�Ӧ��
            }
        }
    }


    private void LateUpdate()
    {
        RecoreUseTime();
    }


    float timeSpan = 300f;//ʱ������
    private float startTime = 0; //��ʱʱ��
    private bool Validatestate = true;

    void RecoreUseTime()
    {
        ////ͬ���û���������
        float elapsedTime1 = Time.time - startTime;
        if (elapsedTime1 > timeSpan && Validatestate)//ͬ��������
        {
            PlayerPrefs.SetString(LastValidTime, DateTime.UtcNow.ToString());
            startTime = Time.time;
        }
    }

    bool ValidateTime()
    {
        string _tmpdatelast = PlayerPrefs.GetString(LastValidTime, "");
        DateTime.TryParse(_tmpdatelast, out _lastValidTime);
        if (_lastValidTime > DateTime.UtcNow)
        {
            Validatestate = false;
            // ʱ������쳣
            return false;
        }
        return true;
    }


    string Encrypt(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = new byte[16]; // ����һ���̶�����Կ
            aesAlg.IV = new byte[16]; // ����һ���̶���IV
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(cipherBytes);
        }
    }

    string Decrypt(string cipherText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = new byte[16]; // �����ʱ��ͬ����Կ
            aesAlg.IV = new byte[16]; // �����ʱ��ͬ��IV
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
