﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Valit;

namespace Valit
{
    internal class ValitRule<TObject, TProperty> : IValitRule<TObject, TProperty>, IValitRuleAccessor<TObject, TProperty> where TObject : class
    {
		public ValitRulesStrategies Strategy => _strategy;

        Func<TObject, TProperty> IValitRuleAccessor<TObject, TProperty>.PropertySelector => _propertySelector;
        IValitRule<TObject, TProperty> IValitRuleAccessor<TObject, TProperty>.PreviousRule => _previousRule;

        private readonly Func<TObject, TProperty> _propertySelector;
        private Predicate<TProperty> _predicate;
        private readonly List<Func<bool>> _conditions;
        private readonly IValitRule<TObject, TProperty> _previousRule;
		private readonly List<string> _errorMessages;
        private readonly ValitRulesStrategies _strategy;

        internal ValitRule(IValitRule<TObject, TProperty> previousRule) : this()
        {
            var previousRuleAccessor = previousRule.GetAccessor();
            _propertySelector = previousRuleAccessor.PropertySelector;
            _previousRule = previousRule;
            _strategy = previousRule.Strategy;
        }

        internal ValitRule(Func<TObject, TProperty> propertySelector, ValitRulesStrategies strategy) : this()
        {
            _propertySelector = propertySelector;
            _strategy = strategy;
        }

        private ValitRule()
        {            
            _errorMessages = new List<string>();
            _conditions = new List<Func<bool>>();
        }		

		void IValitRuleAccessor<TObject, TProperty>.SetPredicate(Predicate<TProperty> predicate)
		{
			_predicate = predicate;
		}

		void IValitRuleAccessor.AddErrorMessage(string message)
		{
			_errorMessages.Add(message);
		}

		void IValitRuleAccessor.AddCondition(Func<bool> predicate)
		{
			_conditions.Add(predicate);
		}

		public IValitResult Validate(TObject @object)
		{
            var property = _propertySelector(@object);
			var hasAllConditionsFulfilled = true;

            foreach(var condition in _conditions)
                hasAllConditionsFulfilled &= condition();

            var isSatisfied = (_predicate == null) ? true : _predicate(property);

            return hasAllConditionsFulfilled && !isSatisfied? ValitResult.CreateFailed(_errorMessages.ToArray()) : ValitResult.CreateSucceeded();		
        }
	}
}
