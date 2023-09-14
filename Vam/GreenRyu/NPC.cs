using UnityEngine;

namespace MVRPlugin
{
    public class NPC : MVRScript
    {
        private bool isEnabled;

        //public HttpResponseMessage? ApiResponse { get; set; }
        //public HttpRequestMessage? ApiRequest { get; set; }
        public JSONStorableString? LocalNpcName { get; set; }
        public JSONStorableFloat? LocalMoveSpeed { get; set; }
        public JSONStorableFloat? LocalRotateSpeed { get; set; }
        public JSONStorableString? LocalResponseMessage { get; set; }
        public JSONStorableString? LocalNpcPrompt { get; set; }
        public JSONStorableUrl? InitNpcUrl { get; set; }
        public JSONStorableUrl? InstructNpcUrl { get; set; }
        public JSONStorableUrl? PromptNpcUrl { get; set; }
        public VRWebBrowser? WebBrowser { get; set; }
        public VRWebBrowser.QuickSite InitNpcSite { get; set; }
        public VRWebBrowser.QuickSite InstructNpcSite { get; set; }
        public VRWebBrowser.QuickSite PromptNpcSite { get; set; }
        public SpeechBubbleControl SpeechBubble { get; set; } = new SpeechBubbleControl();
        public Transform? LocalPlayer { get; set; }

        public override void Init()
        {
            try
            {
                LocalMoveSpeed = new JSONStorableFloat(nameof(LocalMoveSpeed), 5f, 0f, 15f);
                LocalRotateSpeed = new JSONStorableFloat(nameof(LocalRotateSpeed), 5f, 0f, 15f);
                LocalNpcPrompt = new JSONStorableString(nameof(LocalNpcPrompt), "");
                LocalNpcName = new JSONStorableString(nameof(LocalNpcName), "Lexi");
                InitNpcUrl = new JSONStorableUrl(nameof(InitNpcUrl), @"https://localhost:7230/npc/init?gameName=Vam");
                InstructNpcUrl = new JSONStorableUrl(nameof(InstructNpcUrl), @"https://localhost:7230/npc/instruction?gameName=Vam");
                PromptNpcUrl = new JSONStorableUrl(nameof(PromptNpcUrl), @"https://localhost:7230/npc/prompt?gameName=Vam");                
                InitNpcSite = new VRWebBrowser.QuickSite
                {
                    name = nameof(InitNpcSite),
                    url = InitNpcUrl.val,
                };
                InstructNpcSite = new VRWebBrowser.QuickSite
                {
                    name = nameof(InstructNpcSite),
                    url = InstructNpcUrl.val,
                };
                PromptNpcSite = new VRWebBrowser.QuickSite
                {
                    name = nameof(PromptNpcUrl),
                    url = PromptNpcUrl.val,
                };
                WebBrowser = new VRWebBrowser
                {
                    name = $"SyntNetVoiceApi_{nameof(VRWebBrowser)}",
                    enabled = true                       
                };
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
                isEnabled = false;
            }
        }

        /// <summary>
        /// Start is called once before Update or FixedUpdate is called and after Init()
        /// </summary>
        private void Start()
        {
            try
            {
                isEnabled = InitNpc();
                if (isEnabled)
                {
                    LocalPlayer = GameObject.FindGameObjectWithTag("Player").transform;
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
                isEnabled = false;
            }
        }

        /// <summary>
        ///  Update is called with each rendered frame by Unity
        /// </summary>
        private void Update()
        {
            try
            {
                if (isEnabled)
                {
                    if (LocalNpcPrompt != null && LocalNpcPrompt.val != null)
                    {
                        AskQuestion();
                    }
                    else
                    {
                        RotateAndMoveToPlayer();
                    }
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
                isEnabled = false;
            }
        }

        private void AskQuestion()
        {
            try
            {
                if (PromptNpcUrl != null && PromptNpcUrl.val != null && LocalNpcName != null && LocalNpcName.val != null)
                {
                    string question = WWW.EscapeURL(PromptNpcUrl.val);
                    string npcName = WWW.EscapeURL(LocalNpcName.val);
                    string queryParams = $@"&npcName={npcName}&scribe=true&gpt=false&question={question}";
                    PromptNpcUrl.val += queryParams;
                   var ApiResponse = GetRequest(PromptNpcUrl);
                    if (ApiResponse != null)
                    {                     
                        if (SpeechBubble != null)
                        {
                            SpeechBubble.transform.parent = transform;
                            SpeechBubble.UpdateText(ApiResponse.ToString(), 5f);
                        }
                    }
                    else
                    {
                        SuperController.LogMessage($"Problem to ask {npcName}: {question} - The plugin will be disabled!");
                        isEnabled = false;
                    }
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
                isEnabled = false;
            }
        }

        private void RotateAndMoveToPlayer()
        {
            try
            {
                if (LocalPlayer != null && LocalRotateSpeed != null && LocalMoveSpeed != null)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(LocalPlayer.position - transform.position), LocalRotateSpeed.val * Time.deltaTime);
                    transform.position += transform.forward * LocalMoveSpeed.val * Time.deltaTime;
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);               
            }
        }

        private bool InitNpc()
        {
            try
            {
                if (InitNpcUrl != null && InstructNpcUrl != null && LocalNpcName != null)
                {
                    string npcName = WWW.EscapeURL(LocalNpcName.val);
                    string queryParams = $@"&npcName={npcName}";
                    InitNpcUrl.val += queryParams;
                    if (PostRequest(InitNpcUrl) != null)
                    {
                        InstructNpcUrl.val += queryParams;
                        if (GetRequest(InstructNpcUrl) != null)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
                return false;
            }
        }

        /// <summary>
        /// Get web api request
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private object? GetRequest(JSONStorableUrl uri)
        {
            WWW GetRequestObject = new WWW($@"https://localhost:7230/npc/");
            if (uri != null)
            {
                GetRequestObject = new WWW(uri.val);
            }
            return GetRequestObject;
        }

        /// <summary>
        /// Get web api request
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private object? PostRequest(JSONStorableUrl uri)
        {
            WWW PostRequestObject = new WWW($@"https://localhost:7230/npc/init?gameName=Vam&npcName={LocalNpcName}", new byte[0]);
            if (uri != null)
            {
                PostRequestObject = new WWW(uri.val, new byte[0]);
            }
            return PostRequestObject;
        }
    }
}