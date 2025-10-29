using MAP2ADAMOINT.Models.Adamo;
using MAP2ADAMOINT.Models.MapTool;

namespace MAP2ADAMOINT.Services
{
    public class DataMapperService
    {
        private readonly ILogger<DataMapperService> _logger;

        public DataMapperService(ILogger<DataMapperService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Maps a MAP Tool Molecule to ADAMO MapInitial
        /// </summary>
        public MapInitial MapMoleculeToMapInitial(Molecule molecule, Map1_1MoleculeEvaluation? evaluation = null)
        {
            _logger.LogInformation("Mapping Molecule {GrNumber} to MapInitial", molecule.GrNumber);

            return new MapInitial
            {
                GrNumber = molecule.GrNumber ?? "",
                Chemist = molecule.ChemistName,
                EvaluationDate = evaluation?.CreatedAt ?? DateTime.Now, // TODO: verify correct date mapping
                Odor0H = evaluation?.Odor0h,
                Odor4H = evaluation?.Odor4h,
                Odor24H = evaluation?.Odor24h,
                Dilution = "10% in DPG", // TODO: verify default dilution
                Comments = $"Mapped from MAP Tool - Status: {molecule.Status}",
                RegNumber = molecule.RegNumber,
                // Batch extracted from GrNumber by Oracle trigger
                CreatedBy = molecule.CreatedBy ?? "SYNC",
                CreationDate = DateTime.Now
            };
        }

        /// <summary>
        /// Maps ADAMO MapResult to MAP Tool Assessment
        /// </summary>
        public Assessment MapResultToAssessment(MapResult result, MapSession session)
        {
            _logger.LogInformation("Mapping MapResult {ResultId} to Assessment", result.ResultId);

            return new Assessment
            {
                SessionName = $"ADAMO Session {session.SessionId}",
                DateTime = session.EvaluationDate ?? DateTime.Now,
                Stage = session.Stage ?? "Unknown",
                Region = session.Region ?? "",
                Segment = session.Segment ?? "",
                Status = 1, // TODO: map status codes appropriately
                IsClosed = false,
                CreatedBy = result.CreatedBy ?? "SYNC",
                CreatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Maps MAP Tool Map1_1MoleculeEvaluation to ADAMO MapResult
        /// </summary>
        public MapResult MapMoleculeEvaluationToResult(Map1_1MoleculeEvaluation molEval, Map1_1Evaluation evaluation, long sessionId)
        {
            _logger.LogInformation("Mapping MoleculeEvaluation {Id} to MapResult", molEval.Id);

            return new MapResult
            {
                SessionId = sessionId,
                GrNumber = molEval.Molecule?.GrNumber ?? "",
                Odor = molEval.Odor0h ?? molEval.Odor4h ?? molEval.Odor24h,
                BenchmarkComments = molEval.Benchmark,
                Result = molEval.ResultCP ?? molEval.ResultFF, // Use CP or FF result
                Dilution = "10%", // TODO: map from dilution solvent
                Sponsor = evaluation.Participants,
                RegNumber = molEval.Molecule?.RegNumber,
                CreatedBy = molEval.CreatedBy ?? "SYNC",
                CreationDate = DateTime.Now
            };
        }

        /// <summary>
        /// Maps ADAMO OdorCharacterization to MAP Tool OdorFamily scores
        /// Note: This is a simplified mapping - full implementation would require
        /// creating OdorDetail records for each descriptor
        /// </summary>
        public Dictionary<string, int> ExtractOdorFamilyScores(OdorCharacterization odorChar)
        {
            _logger.LogInformation("Extracting odor family scores from {GrNumber}", odorChar.GrNumber);

            var scores = new Dictionary<string, int>();

            if (odorChar.AmbergrisFamily.HasValue)
                scores["AMBERGRIS_FAMILY"] = odorChar.AmbergrisFamily.Value;
            
            if (odorChar.AromaticHerbalFamily.HasValue)
                scores["AROMATIC_HERBAL_FAMILY"] = odorChar.AromaticHerbalFamily.Value;
            
            if (odorChar.CitrusFamily.HasValue)
                scores["CITRUS_FAMILY"] = odorChar.CitrusFamily.Value;
            
            if (odorChar.FloralFamily.HasValue)
                scores["FLORAL_FAMILY"] = odorChar.FloralFamily.Value;
            
            if (odorChar.FruityFamily.HasValue)
                scores["FRUITY_FAMILY"] = odorChar.FruityFamily.Value;
            
            if (odorChar.GreenFamily.HasValue)
                scores["GREEN_FAMILY"] = odorChar.GreenFamily.Value;
            
            if (odorChar.MarineFamily.HasValue)
                scores["MARINE_FAMILY"] = odorChar.MarineFamily.Value;
            
            if (odorChar.MuskyAnimalicFamily.HasValue)
                scores["MUSKY_ANIMALIC_FAMILY"] = odorChar.MuskyAnimalicFamily.Value;
            
            if (odorChar.OffOdorsFamily.HasValue)
                scores["OFF_ODORS_FAMILY"] = odorChar.OffOdorsFamily.Value;
            
            if (odorChar.SpicyFamily.HasValue)
                scores["SPICY_FAMILY"] = odorChar.SpicyFamily.Value;
            
            if (odorChar.SweetGourmandFamily.HasValue)
                scores["SWEET_GOURMAND_FAMILY"] = odorChar.SweetGourmandFamily.Value;
            
            if (odorChar.WoodyFamily.HasValue)
                scores["WOODY_FAMILY"] = odorChar.WoodyFamily.Value;

            return scores;
        }

        /// <summary>
        /// Maps ADAMO MapInitial back to MAP Tool Molecule
        /// </summary>
        public Molecule MapInitialToMolecule(MapInitial initial)
        {
            _logger.LogInformation("Mapping MapInitial {GrNumber} to Molecule", initial.GrNumber);

            return new Molecule
            {
                GrNumber = initial.GrNumber,
                RegNumber = initial.RegNumber,
                ChemistName = initial.Chemist,
                Status = MoleculeStatus.Map1, // Default to Map1
                Assessed = true,
                Quantity = 0, // TODO: verify default quantity
                CreatedBy = initial.CreatedBy ?? "SYNC",
                CreatedAt = DateTime.Now
            };
        }
    }
}

