using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

public static class MyFileSystem
{
    public static void LoadTextAsset(Action<Tries> result)
    {
        Dictionary<int, List<string>> m_ListOfWords = new Dictionary<int, List<string>>();

        TextAsset temp = (TextAsset)Resources.Load("TextAsset/wordlist", typeof(TextAsset));

        string bigList = temp.text;

        string[] strings = Regex.Split(bigList, Environment.NewLine);

        List<string> m_Sort = new List<string>(strings);

        m_Sort.Sort();

        // Make it in a Trie Structure..

        Tries m_Trie = new Tries();

        m_Trie.CreateRoot();

        for (int i = 0; i < m_Sort.Count; i++)
        {
            m_Trie.Add(m_Sort[i].ToCharArray());

            result?.Invoke(m_Trie);
        }
    }

    public static void LoadTextLevel(Action<LevelInfo> result)
    {
        TextAsset temp = (TextAsset)Resources.Load("LevelData/levelData", typeof(TextAsset));

        string json = temp.text;

        LevelInfo info = JsonConvert.DeserializeObject<LevelInfo>(json);

        result?.Invoke(info);

    }
}

public class Tries
{
    public Node root;

    public List<string> words = new List<string>();

    private char[] m_Letters = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

    public void CreateRoot()
    {
        root = new Node();
    }

    public void Add(char[] chars)
    {
        Node tempRoot = root;
        int total = chars.Count() - 1;
        for (int i = 0; i < chars.Count(); i++)
        {
            Node newTrie;
            if (tempRoot.children.Keys.Contains(chars[i]))
            {
                tempRoot = tempRoot.children[chars[i]];
            }
            else
            {
                newTrie = new Node();

                if (total == i)
                {
                    newTrie.last = true;
                }

                tempRoot.children.Add(chars[i], newTrie);
                tempRoot = newTrie;
            }
        }
    }

    public List<string> StartsWith(char[] chars)
    {
        Node temp = root;

        bool flag = true;

        words.Clear();

        StringBuilder builder = new StringBuilder(); // 95% sure string builder is of no help in such case, will look up if I have time.

        for (int i = 0; i < chars.Length; i++)
        {
            if (temp.children.ContainsKey(chars[i]))
            {
                temp = temp.children[chars[i]];
            }
            else
            {
                flag = false;
            }
        }

        if (flag) // this means atleast all the chars found.
        {
            builder.Append(chars); // Add the Prefix

            WordForming(temp, builder.ToString());
        }

        return words;
    }

    public List<string> GetAllTheWords()
    {
        Node temp = root;

        words.Clear();

        StringBuilder builder = new StringBuilder();

        WordForming(temp, builder.ToString());

        return words;
    }


    public bool IsStartingWith(char[] letters)
    {
        Node temp = root;

        for (int i = 0; i < letters.Length; i++)
        {
            if (temp.children.ContainsKey(letters[i]))
            {
                temp = temp.children[letters[i]];
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public bool FindWord(char[] chars)
    {
        Node tempRoot = root;

        int total = chars.Length - 1;

        for (int i = 0; i < chars.Length; i++)
        {
            if (tempRoot.children.Keys.Contains(chars[i]))
            {
                tempRoot = tempRoot.children[chars[i]];

                if (total == i)
                {
                    if (tempRoot.last == true)
                    {
                        return true;
                    }
                }
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private void WordForming(Node temp, string builder) // recursive.. Idk Any Better approach? Todo
    {
        StringBuilder x;

        Node finder = temp;

        if (finder.last)
        {
            words.Add(builder);
        }

        for (int i = 0; i < 26; i++)
        {
            x = new StringBuilder(builder);

            char toCheck = m_Letters[i];

            if (finder.children.ContainsKey(toCheck))
            {
                x.Append(toCheck);

                WordForming(finder.children[toCheck], x.ToString());
            }
        }

        return;
    }


}

public class Node
{
    public Dictionary<char, Node> children = new Dictionary<char, Node>();
    public bool last;
}
