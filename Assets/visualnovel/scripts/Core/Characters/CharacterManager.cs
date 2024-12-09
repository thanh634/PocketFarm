using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace CHARACTER
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager instance { get; private set; }
        public Character[] allCharacters => characters.Values.ToArray();
        private Dictionary<string, Character> characters = new Dictionary<string, Character>();

        private CharacterConfigSO config => DialogueSystem.instance.config.CharacterConfigurationAsset;

        private const string CHARACTER_CASTING_ID = " as ";
        private const string CHARACTER_NAME_ID = "<charname>";
        public string characterRootPathFormat => $"Characters/{CHARACTER_NAME_ID}";
        public string characterPrefabNameFormat => $"Character - [{CHARACTER_NAME_ID}]";
        public string characterPrefabPathFormat => $"{characterRootPathFormat}/{characterPrefabNameFormat}";

        [SerializeField] private RectTransform _characterPanel = null;
        public RectTransform characterPanel => _characterPanel;


        private void Awake()
        {
            instance = this;
        }

        public CharacterConfigData GetCharacterConfig(string name, bool getOriginal = false)
        {
            if(!getOriginal)
            {
                Character character = GetCharacter(name);
                if (character != null)
                    return character.config;

            }

            return config.GetConfig(name);
        }

        public Character GetCharacter(string name, bool createIfDoesNotExist = false)
        {
            if(characters.ContainsKey(name.ToLower()))
                return characters[name.ToLower()];
            else if(createIfDoesNotExist)
                return CreateCharacter(name);

            return null;

        }

        public bool HasCharacter(string name) => characters.ContainsKey(name.ToLower());

        public Character CreateCharacter(string characterName, bool revealAfterCreated = false)
        {
            if (characters.ContainsKey(characterName.ToLower()))
            {
                Debug.LogWarning($"A Character called '{characterName}' already exists. Did not create the character.");
                return null;
            }

            CHARACTER_INFO info = GetCharInfo(characterName);

            Character character = CreateCharacterFromInfo(info);

            characters.Add(info.characterName.ToLower(), character);
            
            if(revealAfterCreated )
                character.Show();

            return character;
        }

        private CHARACTER_INFO GetCharInfo(string characterName)
        { 
            CHARACTER_INFO result = new CHARACTER_INFO();

            string[] nameData = characterName.Split(CHARACTER_CASTING_ID, System.StringSplitOptions.RemoveEmptyEntries);

            result.characterName = nameData[0];
            result.castingName = nameData.Length > 1 ? nameData[1] : result.characterName;

            result.config = config.GetConfig(result.castingName);

            result.prefab = GetPrefabForCharacter(result.castingName);

            result.rootCharacterFolder = FormatCharacterPath(characterRootPathFormat, result.castingName);

            return result;
        }

        private GameObject GetPrefabForCharacter(string characterName)
        {
            string prefabPath = FormatCharacterPath(characterPrefabPathFormat, characterName);
            return Resources.Load<GameObject>(prefabPath);
        }

        public string FormatCharacterPath(string path, string characterName) => path.Replace(CHARACTER_NAME_ID, characterName);

        private Character CreateCharacterFromInfo(CHARACTER_INFO info)
        { 
            CharacterConfigData config = info.config;

            switch(config.characterType)
            {
                case Character.CharacterType.Text:
                    return new Character_Text(info.characterName, config);

                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    return new Character_Sprite(info.characterName, config, info.prefab, info.rootCharacterFolder);

                case Character.CharacterType.Live2D:
                    return new Character_Live2D(info.characterName, config, info.prefab, info.rootCharacterFolder);

                default:
                    return null;

            }
        }

        public void SortCharacters()
        {
            List<Character> activeChars = characters.Values.Where(c => c.root.gameObject.activeInHierarchy && c.isVisible).ToList();
            List<Character> inactiveChars = characters.Values.Except(activeChars).ToList();

            activeChars.Sort((a, b) => a.priority.CompareTo(b.priority));
            activeChars.Concat(inactiveChars);

            SortCharacter(activeChars);
        }

        public void SortCharacters(string[] characterNames)
        {
            List<Character> sortedCharacter = new List<Character>();

            sortedCharacter = characterNames
                .Select(name => GetCharacter(name))
                .Where(character => character != null)
                .ToList();

            List<Character> remainingCharacters = characters.Values
                .Except(sortedCharacter)
                .OrderBy(character => character.priority)
                .ToList();

            sortedCharacter.Reverse();

            int startingPriority = remainingCharacters.Count > 0 ? remainingCharacters.Max(c => c.priority) : 0;
            for (int i = 0; i < sortedCharacter.Count; i++)
            {
                Character character = sortedCharacter[i];
                character.SetPriority(startingPriority + i + 1, autoSortCharacterOnUI: false);
            }

            sortedCharacter.Reverse();
            List<Character> allChars = remainingCharacters.Concat(sortedCharacter).ToList();
            SortCharacter(allChars);
        }

        private void SortCharacter(List<Character> charactersSortOrder)
        {
            int i = 0;
            foreach (Character character in charactersSortOrder)
            {
                Debug.Log($"{character.name} priority is: {character.priority}");
                character.root.SetSiblingIndex(i++);
            }
        }

        private class CHARACTER_INFO
        { 
            public string characterName = "";
            public string castingName = "";

            public string rootCharacterFolder = "";

            public CharacterConfigData config = null;

            public GameObject prefab = null;
        }
    }
}