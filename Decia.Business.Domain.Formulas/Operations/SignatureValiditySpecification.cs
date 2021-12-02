using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Formulas.Operations
{
    public class SignatureValiditySpecification : ISignatureValiditySpecification
    {
        public static readonly DeciaDataType InvalidDataType = DeciaDataTypeUtils.InvalidDataType;

        protected SortedDictionary<int, Parameter> m_Parameters;
        protected Dictionary<int, Signature> m_ValidSignatures;

        public SignatureValiditySpecification(IEnumerable<Parameter> parameters, IEnumerable<Signature> validSignatures)
        {
            if (validSignatures.Count() < 1)
            { throw new InvalidOperationException("All operations must have at least one valid signature."); }

            if (parameters.Count() > 0)
            {
                if (!AreIndexesContiguous(parameters.Select(p => p.Index)))
                { throw new InvalidOperationException("The indexes of the parameter list must be contiguous"); }

                IEnumerable<Parameter> required = parameters.Where(p => p.IsRequired);
                IEnumerable<Parameter> optional = parameters.Where(p => !p.IsRequired);
                if ((required.Count() > 0) && (optional.Count() > 0))
                {
                    if (required.Select(p => p.Index).Max() > optional.Select(p => p.Index).Min())
                    { throw new InvalidOperationException("All required parameters must come before any optional parameters."); }
                }

                List<int> parameterMultiples = parameters.Where(p => p.AllowMultiple).Select(p => p.Index).ToList();
                if (parameterMultiples.Count > 1)
                { throw new InvalidProgramException("Only one parameter can allow multiple instances."); }
                if ((parameterMultiples.Count == 1) && (optional.Count() > 1))
                { throw new InvalidProgramException("If a parameter allows multiple instances, the Signature can only contain one optional parameter."); }

                if (parameterMultiples.Count > 0)
                {
                    int multipleIndex = parameterMultiples.First();
                    int maxIndex = parameters.Select(p => p.Index).Max();
                    if (multipleIndex != maxIndex)
                    { throw new InvalidProgramException("Only the last parameter can allow multiple instances."); }
                }
            }

            Dictionary<int, Parameter> parameterDict = parameters.ToDictionary(p => p.Index, p => p);
            Dictionary<int, Signature> validSignaturesDict = new Dictionary<int, Signature>();
            foreach (Signature validSignature in validSignatures)
            {
                if (validSignaturesDict.ContainsKey(validSignature.Key))
                { throw new InvalidOperationException("All signatures must have unique set of Parameter types."); }

                validSignaturesDict.Add(validSignature.Key, validSignature);
            }

            m_Parameters = new SortedDictionary<int, Parameter>(parameterDict);
            m_ValidSignatures = validSignaturesDict;
        }

        public IList<Parameter> Parameters
        {
            get { return m_Parameters.Values.OrderBy(p => p.Index).ToList(); }
        }

        public IList<Parameter> RequiredParameters
        {
            get { return m_Parameters.Values.Where(p => p.IsRequired).OrderBy(p => p.Index).ToList(); }
        }

        public IList<Parameter> OptionalParameters
        {
            get { return m_Parameters.Values.Where(p => !p.IsRequired).OrderBy(p => p.Index).ToList(); }
        }

        public Nullable<Parameter> ParameterAllowingMultipleInstances
        {
            get
            {
                List<Parameter> multiples = m_Parameters.Values.Where(p => p.AllowMultiple).ToList();
                if (multiples.Count < 1)
                { return null; }
                return multiples.First();
            }
        }

        public int ParameterCount
        {
            get { return Parameters.Count; }
        }

        public int RequiredParameterCount
        {
            get { return RequiredParameters.Count; }
        }

        public int OptionalParameterCount
        {
            get { return OptionalParameters.Count; }
        }

        public bool HasParameterAllowingMultipleInstances
        {
            get { return ParameterAllowingMultipleInstances.HasValue; }
        }

        public bool TryGetValidReturnType(IEnumerable<DeciaDataType> desiredInputTypes, out DeciaDataType returnType)
        {
            Dictionary<int, DeciaDataType> desiredInputTypesByIndex = desiredInputTypes.GetAsDictionary();
            return TryGetValidReturnType(desiredInputTypesByIndex, out returnType);
        }

        public bool TryGetValidReturnType(IDictionary<int, DeciaDataType> desiredInputTypes, out DeciaDataType returnType)
        {
            Signature validSignature = null;
            bool isValid = TryGetValidSignature(desiredInputTypes, out validSignature);

            if (!isValid)
            { returnType = InvalidDataType; }
            else
            { returnType = validSignature.TypeOut; }

            return isValid;
        }

        public bool TryGetValidSignature(IEnumerable<DeciaDataType> desiredInputTypes, out Signature signature)
        {
            Dictionary<int, DeciaDataType> desiredInputTypesByIndex = desiredInputTypes.GetAsDictionary();
            return TryGetValidSignature(desiredInputTypesByIndex, out signature);
        }

        public bool TryGetValidSignature(IDictionary<int, DeciaDataType> desiredInputTypes, out Signature signature)
        {
            var signatureGroup_ParamCounts = this.m_ValidSignatures.Values.Select(x => x.TypesIn.Count).Distinct().ToList();
            Nullable<int> prospectiveSignatureKey = null;
            signature = null;


            foreach (var signatureGroup_ParamCount in signatureGroup_ParamCounts)
            {
                var validSignaturesForGroup = m_ValidSignatures.Where(x => x.Value.TypesIn.Count == signatureGroup_ParamCount).ToDictionary(x => x.Key, x => x.Value);
                var isValidForMultiple = ((signatureGroup_ParamCount > 0) && HasParameterAllowingMultipleInstances);

                if ((desiredInputTypes.Count > signatureGroup_ParamCount) && !isValidForMultiple)
                { continue; }

                if (desiredInputTypes.Count <= 0)
                {
                    if (RequiredParameterCount <= 0)
                    {
                        prospectiveSignatureKey = Signature.GetKeyForInputTypes(desiredInputTypes);
                        signature = validSignaturesForGroup[prospectiveSignatureKey.Value];
                        return true;
                    }
                }

                if (!AreIndexesContiguous(desiredInputTypes.Keys))
                { continue; }

                var passed = true;
                foreach (int index in m_Parameters.Keys)
                {
                    Parameter parameter = m_Parameters[index];

                    if (parameter.IsRequired)
                    {
                        if (!desiredInputTypes.ContainsKey(index))
                        {
                            passed = false;
                            break;
                        }
                    }
                }

                if (!passed)
                { continue; }

                if (desiredInputTypes.Count == signatureGroup_ParamCount)
                {
                    prospectiveSignatureKey = Signature.GetKeyForInputTypes(desiredInputTypes);

                    if (!validSignaturesForGroup.ContainsKey(prospectiveSignatureKey.Value))
                    { continue; }

                    signature = validSignaturesForGroup[prospectiveSignatureKey.Value];
                    return true;
                }

                if (desiredInputTypes.Count < signatureGroup_ParamCount)
                {
                    prospectiveSignatureKey = Signature.GetKeyForInputTypes(desiredInputTypes);

                    foreach (Signature validSignature in validSignaturesForGroup.Values)
                    {
                        if (Signature.GetKeyForInputTypes(validSignature.TypesIn, desiredInputTypes.Count) == prospectiveSignatureKey)
                        {
                            signature = validSignature;
                            return true;
                        }
                    }

                    passed = false;
                }

                if (!passed)
                { continue; }

                if ((desiredInputTypes.Count > signatureGroup_ParamCount) && (!HasParameterAllowingMultipleInstances))
                { continue; }


                Signature explicitSignature = null;
                IDictionary<int, DeciaDataType> reducedInputTypes = new SortedDictionary<int, DeciaDataType>(desiredInputTypes);
                for (int i = signatureGroup_ParamCount; i < desiredInputTypes.Count; i++)
                { reducedInputTypes.Remove(i); }
                int reducedSignatureKey = Signature.GetKeyForInputTypes(reducedInputTypes);

                if (!validSignaturesForGroup.ContainsKey(reducedSignatureKey))
                { continue; }
                explicitSignature = validSignaturesForGroup[reducedSignatureKey];

                foreach (Signature inferredSignature in GetInferredSignatures(explicitSignature, desiredInputTypes.Count))
                {
                    bool isComboValid = true;

                    if (inferredSignature.TypesIn.Count != desiredInputTypes.Count)
                    { throw new InvalidOperationException("The number of parameters in the inferred combination should match the number requested."); }

                    foreach (int index in inferredSignature.TypesInByIndex.Keys)
                    {
                        if (inferredSignature.TypesInByIndex[index] != desiredInputTypes[index])
                        {
                            isComboValid = false;
                            break;
                        }
                    }

                    if (isComboValid)
                    {
                        signature = inferredSignature;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsSpecificInstanceOf(Signature baseSignature, Signature inferredSignature)
        {
            return ISignatureValiditySpecificationUtils.IsSpecificInstanceOf(this, baseSignature, inferredSignature);
        }

        private IEnumerable<Signature> GetInferredSignatures(Signature validSignature, int actualParameterCount)
        {
            if (!HasParameterAllowingMultipleInstances)
            { return new Signature[] { validSignature }; }

            SortedDictionary<int, DeciaDataType> extendedInputTypes = new SortedDictionary<int, DeciaDataType>(validSignature.TypesInByIndex);
            SortedDictionary<int, bool> extendedAllowsDynamic = new SortedDictionary<int, bool>(validSignature.AllowsDynamicInByIndex);

            int startIndex = extendedInputTypes.Count;
            int baseIndex = (startIndex - 1);
            DeciaDataType baseDataType = extendedInputTypes[baseIndex];
            bool baseAllowsDynamic = extendedAllowsDynamic[baseIndex];

            for (int i = startIndex; i < actualParameterCount; i++)
            {
                extendedInputTypes.Add(i, baseDataType);
                extendedAllowsDynamic.Add(i, baseAllowsDynamic);
            }
            return new Signature[] { new Signature(extendedInputTypes.Values, extendedAllowsDynamic.Values, validSignature.TypeOut) };
        }

        private bool AreIndexesContiguous(IEnumerable<int> indexes)
        {
            Nullable<int> lastIndex = null;
            foreach (int index in indexes.OrderBy(i => i))
            {
                if (!lastIndex.HasValue)
                {
                    if (index != 0)
                    { return false; }
                }
                else
                {
                    if ((index - lastIndex.Value) != 1)
                    { return false; }
                }
                lastIndex = index;
            }
            return true;
        }
    }
}