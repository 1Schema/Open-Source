MyDb.system.js.save(
  {
    _id: "fnDecia_Time_GetTimePeriodDatesForId",
    value: function (timePeriodId) {
      var datesAsString = timePeriodId.split("_");

      var startDateAsString = datesAsString[0];
      var endDateAsString = datesAsString[1];

      var startDate = fnDecia_Utility_GetValueAsDate(startDateAsString);
      var endDate = fnDecia_Utility_GetValueAsDate(endDateAsString);

      return { StartDate: startDate, EndDate: endDate };
    }
  }
);