MyDb.system.js.save(
  {
    _id: "fnDecia_Time_GetNextTimePeriodStartDate",
    value: function (timePeriodTypeId, currentDate) {
      var MyDb = db.getSiblingDB("<MY_DB_NAME>");

      currentDate = fnDecia_Utility_GetValueAsDate(currentDate);
      var nextDate = new Date(currentDate);

      var timePeriodTypesCollection = MyDb.getCollection("Decia_TimePeriodTypes");
      var timePeriodTypesCursor = timePeriodTypesCollection.find({ _id: timePeriodTypeId });
      var timePeriodType = timePeriodTypesCursor.hasNext() ? timePeriodTypesCursor.next() : null;

      if (timePeriodType.Name == "Years") {
        nextDate = new Date(nextDate.setFullYear(currentDate.getFullYear() + 1));
        return new Date(nextDate);
      }
      if (timePeriodType.Name == "HalfYears") {
        nextDate = new Date(nextDate.setMonth(currentDate.getMonth() + 6));
        return new Date(nextDate);
      }
      if (timePeriodType.Name == "QuarterYears") {
        nextDate = new Date(nextDate.setMonth(currentDate.getMonth() + 3));
        return new Date(nextDate);
      }
      if (timePeriodType.Name == "Months") {
        nextDate = new Date(nextDate.setMonth(currentDate.getMonth() + 1));
        return new Date(nextDate);
      }

      var nextMonthDate = new Date(nextDate.setMonth(currentDate.getMonth() + 1));
      var durationInMs = (nextMonthDate.getTime() - currentDate.getTime());

      nextDate = new Date(currentDate);

      if (timePeriodType.Name == "HalfMonths") {
        nextDate = new Date(nextDate.setMilliseconds(durationInMs / 2.0));
        return new Date(nextDate);
      }
      if (timePeriodType.Name == "QuarterMonths") {
        nextDate = new Date(nextDate.setMilliseconds(durationInMs / 4.0));
        return new Date(nextDate);
      }
      if (timePeriodType.Name == "Days") {
        nextDate = new Date(nextDate.setDate(currentDate.getDate() + 1));
        return new Date(nextDate);
      }

      throw "Unrecognized TimePeriodType encountered!";
    }
  }
);