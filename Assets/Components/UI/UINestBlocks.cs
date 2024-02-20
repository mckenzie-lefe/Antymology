using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Antymology.UI
{
    public class UINestBlocks : MonoBehaviour
    {
        private int numberOfNestBlocks = 0;
        private TextMeshProUGUI nestBlockText;

        private void Awake()
        {
            nestBlockText = GetComponent<TextMeshProUGUI>();
        }

        public void UpdateNestBlocks()
        {
            numberOfNestBlocks++;
            nestBlockText.text = "Nest Blocks: " + numberOfNestBlocks;
        }
    }
}
