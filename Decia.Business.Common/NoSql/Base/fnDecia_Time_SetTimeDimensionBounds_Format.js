MyDb.system.js.save(
  {
    _id: "fnDecia_Time_SetTimeDimensionBounds",
    value: function (startDate, endDate, writeConcern) {
      var MyDb = db.getSiblingDB("<MY_DB_NAME>");

      startDate = fnDecia_Utility_GetValueAsDate(startDate);
      endDate = fnDecia_Utility_GetValueAsDate(endDate);

      fnDecia_ChangeState_IncrementLatest(writeConcern);

      var minDateIsFirstStart = true;
      var endDateDiffInMs = -3;

      var variableTemplatesCollection = MyDb.getCollection("Decia_VariableTemplates");
      var variableTemplatesCursor = timePeriodTypesCollection.find();
      var usedTimePeriodTypeIds = new Array();

      while (variableTemplatesCursor.hasNext()) {
        var variableTemplate = variableTemplatesCursor.next();

        if (variableTemplate.TimeDimensionCount >= 1) {
          usedTimePeriodTypeIds.push(variableTemplate.PrimaryTimePeriodTypeId);
        }
        if (variableTemplate.TimeDimensionCount >= 2) {
          usedTimePeriodTypeIds.push(variableTemplate.SecondaryTimePeriodTypeId);
        }
      }

      var timePeriodTypesCollection = MyDb.getCollection("Decia_TimePeriodTypes");
      var usedTimePeriodTypesCursor = timePeriodTypesCollection.find({ _id: { $in: usedTimePeriodTypeIds } });
      var usedTimePeriodTypes = new Array();

      while (usedTimePeriodTypesCursor.hasNext()) {
        var timePeriodType = usedTimePeriodTypesCursor.next();
        usedTimePeriodTypes.push(timePeriodType);
      }

      if (usedTimePeriodTypes.length < 1) {
        return;
      }

      var timePeriodIds = new Array();
      var timePeriods = {};

      for (var i = 0; i < usedTimePeriodTypes.length; i++) {
        var timePeriodType = usedTimePeriodTypes[i];
        var timePeriodTypeId = timePeriodType._id;

        var timePeriodStartDate = new Date(startDate);
        var currentIndex = 0;

        while (timePeriodStartDate < endDate) {
          var timePeriodEndDate = null;
          var timePeriodId = null;
          var timePeriod = null;

          timePeriodEndDate = fnDecia_GetTimePeriodNextDate(timePeriodTypeId, timePeriodStartDate);
          timePeriodEndDate = new Date(timePeriodEndDate.setMilliseconds(endDateDiffInMs));

          timePeriodId = fnDecia_GetTimePeriodId_ForDates(timePeriodStartDate, timePeriodEndDate);
          timePeriod = { _id: timePeriodId, TimePeriodTypeId: timePeriodTypeId, StartDate: timePeriodStartDate, EndDate: timePeriodEndDate, IsForever: 0 };

          timePeriodIds.push(timePeriod._id);
          timePeriods[timePeriod._id] = timePeriod;

          timePeriodStartDate = fnDecia_GetTimePeriodNextDate(timePeriodTypeId, timePeriodStartDate);
          currentIndex++;
        }
      }

      var timePeriodsCollection = MyDb.getCollection("Decia_TimePeriods");
      timePeriodsCollection.deleteMany({ _id: { $nin: timePeriodIds } }, writeConcern);

      for (var timePeriodId in timePeriods) {
        timePeriod = timePeriods[timePeriodId];

        var tpsCursor = timePeriodsCollection.find({ _id: timePeriodId });
        if (tpsCursor.hasNext()) {
          continue;
        }

        timePeriodsCollection.insertOne(timePeriod, writeConcern);
      }

      var timeDimensionSettingsCollection = MyDb.getCollection("Decia_TimeDimensionSettings");
      var timeDimensionSettingsCursor = timeDimensionSettingsCollection.find();

      var timeDimensionSetting = { _id: 1, StartDate: startDate, EndDate: endDate };

      if (timeDimensionSettingsCursor.hasNext()) {
        timeDimensionSetting = timeDimensionSettingsCursor.next();
        timeDimensionSetting.StartDate = startDate;
        timeDimensionSetting.EndDate = endDate;
      }

      timeDimensionSettingCollection.save(timeDimensionSetting, writeConcern);

      fnDecia_ChangeState_IncrementLatest(writeConcern);
    }
  }
);