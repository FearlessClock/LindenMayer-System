using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LindenMayer_System
{
    class LindenmayerSystem
    {
        //The string that gets returned after being filled with the new axiom
        string word;
        //List of all the regular rules
        List<RegularGrammer> regularRules = new List<RegularGrammer>();
        //List of all the Stochastic rules
        List<StochasticGrammar> stochasticRules = new List<StochasticGrammar>();
        //List of all the Context Sensitive rules
        List<ContextSensitiveGrammer> cSentiveRules = new List<ContextSensitiveGrammer>();
        //All the variables in a given string
        char[] variables = new char[7];
        //All the char that don't have rules acting on them
        char[] constantes = new char[8] { '[', ']', 'L', 'R', '+', '-', 'G', 'D' };

        Random rand = new Random();

        XmlDocument settings;

        public LindenmayerSystem(string fileName)
        {
            settings = new XmlDocument();
            settings.Load(fileName);
            XmlNode nodes = settings.SelectSingleNode("//LindenMayerSystem/Rules/Variables");
            List<char> vars = new List<char>();
            variables = nodes.Attributes["vars"].Value.ToCharArray();
            SetupTheRegularRules();
            SetupTheStochasticRules();
            SetupTheCSensitiveRules();
        }

        private void SetupTheStochasticRules()
        {
            
            XmlNodeList nodes = settings.SelectNodes("//LindenMayerSystem/Rules/StochasticGrammar/rule");
            foreach(XmlNode rule in nodes)
            {
                stochasticRules.Add(new StochasticGrammar(rule.Attributes["toReplace"].Value.ToCharArray()[0], 
                                                          rule.Attributes["replaceBy1"].Value, 
                                                          rule.Attributes["replaceBy2"].Value, 
                                                          rule.Attributes["replaceBy3"].Value, 
                                                          Convert.ToDouble(rule.Attributes["chanceToReplace1"].Value)/10, 
                                                          Convert.ToDouble(rule.Attributes["chanceToReplace2"].Value)/10));
            }
        }

        private void SetupTheRegularRules()
        {
            XmlNodeList nodes = settings.SelectNodes("//LindenMayerSystem/Rules/RegularGrammer/rule");
            foreach (XmlNode rule in nodes)
            {
                regularRules.Add(new RegularGrammer(rule.Attributes["toReplace"].Value.ToCharArray()[0], rule.Attributes["replaceBy"].Value));
            }
        }

        private void SetupTheCSensitiveRules()
        {
            
            XmlNodeList nodes = settings.SelectNodes("//LindenMayerSystem/Rules/ContextSensitiveGrammer/rule");
            foreach (XmlNode rule in nodes)
            {
                cSentiveRules.Add(new ContextSensitiveGrammer(rule.Attributes["before"].Value.ToCharArray()[0], rule.Attributes["toReplace"].Value.ToCharArray()[0], rule.Attributes["after"].Value.ToCharArray()[0], rule.Attributes["replaceBy"].Value));
            }

        }

        private bool CheckVariable(char a)
        {
            foreach(char v in variables)
            {
                if (v == a)
                    return true;
            }
            return false;
        }
        private bool CheckConstant(char a)
        {
            throw new NotImplementedException("CheckConstant");
        }

        public string Calculate(string stringToConvert)
        {
            word = stringToConvert;
            char[] axiomEvo = word.ToArray<char>();
            List<char> produces = new List<char>();

            produces.AddRange(CheckVariable(axiomEvo[0]) ? ApplyRule((char)1, axiomEvo[0], (char)1) : new char[1] { axiomEvo[0] });
            for(int i = 1; i < axiomEvo.Length-1; i++)
            {
                produces.AddRange(CheckVariable(axiomEvo[i]) ? ApplyRule(axiomEvo[i-1], axiomEvo[i], axiomEvo[i + 1]) : new char[1] { axiomEvo[i] });
            }
            if(axiomEvo.Length -1 != 0)
                produces.AddRange(CheckVariable(axiomEvo[axiomEvo.Length - 1]) ? ApplyRule((char)1, axiomEvo[axiomEvo.Length-1], (char)1) : new char[1] { axiomEvo[axiomEvo.Length - 1] });
            //foreach (char c in axiomEvo)
            //{
            //    produces.AddRange(CheckVariable(c)? ApplyRule(c):new char[1] { c });
            //}
            axiomEvo = produces.ToArray<char>();
            word = "";
            foreach(char c in axiomEvo)
            {
                word += c;
            }
            return word;
        }

        /// <summary>
        /// Apply all the rules to the passed char 
        /// </summary>
        /// <param name="whatAreWeLookingAt">The char that has to be changed</param>
        /// <returns></returns>
        private char[] ApplyRule(char before, char whatAreWeLookingAt, char after)
        {
            //Priorities need to be set, I need to decide which one I do first
            //My chose is; CSG, RG, SG
            foreach (ContextSensitiveGrammer csg in cSentiveRules)
            {
                if (csg.ContextGood(before, whatAreWeLookingAt, after))
                {
                    return csg.ReplaceWith.ToArray<char>();
                }
            }
            foreach (RegularGrammer reg in regularRules)
            {
                if (reg.toReplace.Equals(whatAreWeLookingAt))
                {
                    return reg.replaceBy;
                }
            }
            foreach (StochasticGrammar sto in stochasticRules)
            {
                if (sto.toReplace.Equals(whatAreWeLookingAt))
                {
                    return sto.GetReplacer(rand.NextDouble());
                }
            }
            return new char[1] { whatAreWeLookingAt };
        }
    }
}
