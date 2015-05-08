using System.Collections.Generic;
using System.Linq;
using java.io;
using opennlp.tools.namefind;
using opennlp.tools.sentdetect;
using opennlp.tools.tokenize;
using opennlp.tools.util;

namespace IFilterTextViewer
{
    public class EntityExtractor
    {
        public enum EntityType
        {
            Date = 0,
            Location,
            Money,
            Organization,
            Person,
            Time
        }

        private string _nameFinderModelPath; //NameFinder model path for English names

        /// <summary>
        ///     Extractor for the entity types available in openNLP.
        ///     Copyright 2013, Don Krapohl www.augmentedintel.com
        ///     This source is free for unlimited distribution and use
        ///     TODO:
        ///     try/catch/exception handling
        ///     filestream closure
        ///     model training if desired
        ///     Regex or dictionary entity extraction
        ///     clean up the setting of the Name Finder model path
        /// </summary>
        /// Call syntax:  myList = ExtractEntities(myInText, EntityType.Person);
        private const string SentenceModelPath = "d:\\models\\nl-sent.bin"; //path to the model for sentence detection

        private const string TokenModelPath = "d:\\models\\nl-token.bin"; //model path for English tokens

        public string[] ParseSenteces(string inputData)
        {
            //initialize the sentence detector
            var sentenceParser = PrepareSentenceDetector();
            var sentences = sentenceParser.sentDetect(inputData);
            //detect the sentences and load into sentence array of strings
            return sentences;
        }

        public List<string> ExtractEntities(string inputData, EntityType targetType)
        {
            /*required steps to detect names are:
             * downloaded sentence, token, and name models from http://opennlp.sourceforge.net/models-1.5/
             * 1. Parse the input into sentences
             * 2. Parse the sentences into tokens
             * 3. Find the entity in the tokens
 
            */

            //------------------Preparation -- Set Name Finder model path based upon entity type-----------------
            switch (targetType)
            {
                case EntityType.Date:
                    _nameFinderModelPath = "d:\\models\\nl-ner-date.bin";
                    break;
                case EntityType.Location:
                    _nameFinderModelPath = "d:\\models\\nl-ner-location.bin";
                    break;
                case EntityType.Money:
                    _nameFinderModelPath = "d:\\models\\nl-ner-money.bin";
                    break;
                case EntityType.Organization:
                    _nameFinderModelPath = "d:\\models\\nl-ner-organization.bin";
                    break;
                case EntityType.Person:
                    _nameFinderModelPath = "d:\\models\\nl-ner-person.bin";
                    break;
                case EntityType.Time:
                    _nameFinderModelPath = "d:\\models\\nl-ner-time.bin";
                    break;
            }

            //initialize the sentence detector
            var sentenceParser = PrepareSentenceDetector();

            //initialize person names model
            var nameFinder = PrepareNameFinder();

            //initialize the tokenizer--used to break our sentences into words (tokens)
            var tokenizer = PrepareTokenizer();

            var sentences = sentenceParser.sentDetect(inputData);
                //detect the sentences and load into sentence array of strings
            var results = new List<string>();

            foreach (var sentence in sentences)
            {
                //now tokenize the input.
                //"Don Krapohl enjoys warm sunny weather" would tokenize as
                //"Don", "Krapohl", "enjoys", "warm", "sunny", "weather"
                var tokens = tokenizer.tokenize(sentence);

                //do the find
                var foundNames = nameFinder.find(tokens);

                //important:  clear adaptive data in the feature generators or the detection rate will decrease over time.
                nameFinder.clearAdaptiveData();

                results.AddRange(Span.spansToStrings(foundNames, tokens).AsEnumerable());
            }

            return results;
        }

        #region private methods
        private static TokenizerME PrepareTokenizer()
        {
            var tokenInputStream = new FileInputStream(TokenModelPath); //load the token model into a stream
            var tokenModel = new TokenizerModel(tokenInputStream); //load the token model
            return new TokenizerME(tokenModel); //create the tokenizer
        }

        private static SentenceDetectorME PrepareSentenceDetector()
        {
            var sentModelStream = new FileInputStream(SentenceModelPath); //load the sentence model into a stream
            var sentModel = new SentenceModel(sentModelStream); // load the model
            return new SentenceDetectorME(sentModel); //create sentence detector
        }

        private NameFinderME PrepareNameFinder()
        {
            var modelInputStream = new FileInputStream(_nameFinderModelPath); //load the name model into a stream
            var model = new TokenNameFinderModel(modelInputStream); //load the model
            return new NameFinderME(model); //create the namefinder
        }
        #endregion
    }
}