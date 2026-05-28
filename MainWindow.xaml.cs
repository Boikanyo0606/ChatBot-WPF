using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatBot_WPF
{
    public partial class MainWindow : Window
    {
        private Chatbot bot;
        private SpeechSynthesizer speaker;

        public MainWindow()
        {
            InitializeComponent();

            speaker = new SpeechSynthesizer();
            speaker.Volume = 100;
            speaker.Rate = 0;

            bot = new Chatbot();

            SetAsciiArt();
            WelcomeMessage();
        }

        private void SetAsciiArt()
        {
            string ascii = @"
    ╔═══════════════════════════════════════════════════════════╗
    ║          ██████╗██╗   ██╗██████╗ ███████╗██████╗        ║
    ║         ██╔════╝██║   ██║██╔══██╗██╔════╝██╔══██╗       ║
    ║        ██║     ██║   ██║██████╔╝█████╗  ██████╔╝       ║
             ██║     ██║   ██║██╔══██╗██╔══╝  ██╔══██╗       ║
             ╚██████╗██████╝██║  ██║███████╗██║  ██║       ║
              ═════╝ ═════╝ ╚═╝  ═╝══════╝═╝  ╚═╝       
    ║              CYBERSECURITY AWARENESS BOT                ║
    ╚═══════════════════════════════════════════════════════════╝";

            txtAscii.Text = ascii;
        }

        private void WelcomeMessage()
        {
            speaker.SpeakAsync("Hello! Welcome to the Cybersecurity Awareness Bot.");
            txtChat.AppendText("Bot: Hello! I'm here to help you stay safe online. Ask me about: passwords, scams, phishing, privacy, security, or malware. What's your name?" + Environment.NewLine);
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            string input = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            txtChat.AppendText("You: " + input + Environment.NewLine);
            txtMessage.Clear();

            string response = bot.GetResponse(input);
            txtChat.AppendText("Bot: " + response + Environment.NewLine);

            speaker.SpeakAsync(response);
            txtChat.ScrollToEnd();
        }

        
    }

    public class Chatbot
    {
        private Dictionary<string, string> userMemory = new Dictionary<string, string>();
        private string currentTopic = "";
        private string lastResponse = "";

        private Dictionary<string, List<string>> keywordResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["password"] = new List<string>
            {
                "Make sure to use strong, unique passwords for each account. Avoid using personal details.",
                "Consider using a password manager to generate and store complex passwords securely.",
                "Enable multi-factor authentication wherever possible for an extra layer of security.",
                "A strong password should be at least 12 characters with uppercase, lowercase, numbers, and symbols."
            },
            ["scam"] = new List<string>
            {
                "Be cautious of unsolicited emails asking for personal information. Scammers often disguise themselves.",
                "Never click on suspicious links. Hover over them to see the actual URL before clicking.",
                "Legitimate organizations will never ask for sensitive information via email unexpectedly.",
                "If an offer seems too good to be true, it probably is a scam."
            },
            ["phishing"] = new List<string>
            {
                "Be cautious of emails asking for personal information. Verify the sender's address.",
                "Check for spelling and grammar errors - they're common in phishing attempts.",
                "Never enter login credentials on a page you reached via an email link.",
                "Phishing emails often create a sense of urgency to make you act without thinking."
            },
            ["privacy"] = new List<string>
            {
                "Review privacy settings on your social media accounts regularly.",
                "Be mindful of what you share online. Once posted, it's hard to remove completely.",
                "Use privacy-focused browsers and search engines to protect your activity.",
                "Avoid using public Wi-Fi for sensitive transactions without a VPN."
            },
            ["security"] = new List<string>
            {
                "Keep your software and operating system updated to protect against vulnerabilities.",
                "Use reputable antivirus software and keep it updated.",
                "Enable two-factor authentication on all important accounts.",
                "Regularly backup your important files to an external drive or cloud storage."
            },
            ["malware"] = new List<string>
            {
                "Only download software from trusted sources to avoid malware infections.",
                "Be careful with email attachments, even from known contacts.",
                "Run regular scans with your antivirus software.",
                "Keep your firewall enabled to block unauthorized access."
            }
        };

        private Dictionary<string, string> sentimentKeywords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["worried"] = "concerned",
            ["anxious"] = "concerned",
            ["scared"] = "concerned",
            ["afraid"] = "concerned",
            ["curious"] = "curious",
            ["interested"] = "curious",
            ["want to know"] = "curious",
            ["wonder"] = "curious",
            ["frustrated"] = "frustrated",
            ["annoyed"] = "frustrated",
            ["confused"] = "frustrated",
            ["overwhelmed"] = "frustrated"
        };

        public string GetResponse(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return "I didn't catch that. Could you please type your question?";

            if (!userMemory.ContainsKey("name"))
            {
                userMemory["name"] = userInput.Trim('.', ' ');
                return "Nice to meet you, " + userMemory["name"] + "! I'm here to help with cybersecurity. What would you like to know?";
            }

            if (userInput.IndexOf("interested in", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                int index = userInput.IndexOf("interested in", StringComparison.OrdinalIgnoreCase);
                string interest = userInput.Substring(index + 15).Split(' ', '.', ',')[0];
                if (!string.IsNullOrEmpty(interest))
                {
                    userMemory["interest"] = interest.Trim();
                    return "Great! I'll remember you're interested in " + userMemory["interest"] + ". What specific aspect would you like to learn about?";
                }
            }

            if (IsFollowUpQuestion(userInput))
                return HandleFollowUp();

            string sentiment = DetectSentiment(userInput);
            string keywordResponse = FindKeywordResponse(userInput, sentiment);
            if (keywordResponse != null)
                return keywordResponse;

            if (userMemory.ContainsKey("interest") && new Random().Next(3) == 0)
                return "Since you're interested in " + userMemory["interest"] + ", remember that staying informed is key. What specific aspect would you like to know more about?";

            if (userMemory.ContainsKey("name") && new Random().Next(3) == 0)
                return "I'm not sure I understand, " + userMemory["name"] + ". Can you try rephrasing? You can ask about: passwords, scams, phishing, privacy, security, or malware.";

            return GetDefaultResponse(sentiment);
        }

        private string DetectSentiment(string input)
        {
            foreach (KeyValuePair<string, string> kw in sentimentKeywords)
            {
                if (input.IndexOf(kw.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                    return kw.Value;
            }
            return "neutral";
        }

        private string FindKeywordResponse(string input, string sentiment)
        {
            foreach (string kw in keywordResponses.Keys)
            {
                if (input.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    currentTopic = kw;
                    List<string> responses = keywordResponses[kw];
                    lastResponse = responses[new Random().Next(responses.Count)];
                    return AddSentimentAdjustment(lastResponse, sentiment);
                }
            }
            return null;
        }

        private string AddSentimentAdjustment(string response, string sentiment)
        {
            if (sentiment == "concerned")
                return "It's understandable to feel that way. " + response + " Let me know if you need more help.";
            if (sentiment == "frustrated")
                return "I understand this can be frustrating. " + response + " Take your time.";
            if (sentiment == "curious")
                return "Great question! " + response + " Would you like to know more?";
            return response;
        }

        private bool IsFollowUpQuestion(string input)
        {
            string[] phrases = { "tell me more", "explain more", "give me another", "more information", "what else", "anything else" };
            foreach (string p in phrases)
            {
                if (input.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private string HandleFollowUp()
        {
            if (!string.IsNullOrEmpty(currentTopic) && keywordResponses.ContainsKey(currentTopic))
            {
                List<string> responses = keywordResponses[currentTopic];
                Random random = new Random();
                string newResp = responses[random.Next(responses.Count)];
                while (newResp == lastResponse && responses.Count > 1)
                    newResp = responses[random.Next(responses.Count)];
                lastResponse = newResp;
                return "Here's another tip about " + currentTopic + ": " + newResp;
            }
            return "I'd be happy to provide more information. Which topic would you like to know more about?";
        }

        private string GetDefaultResponse(string sentiment)
        {
            if (sentiment == "concerned")
                return "I understand your concerns. What aspect of cybersecurity would you like to know about? I can help with passwords, scams, phishing, privacy, security, and malware.";
            if (sentiment == "frustrated")
                return "I understand this can be overwhelming. Let's take it step by step. What topic would you like to learn about?";
            return "I'm not sure I understand. Try asking about: passwords, scams, phishing, privacy, security, or malware.";
        }

        public string GeneratePassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            Random random = new Random();
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            return new string(result);
        }
    }
}