MyDb.system.js.save(
  {
    _id: "fnDecia_Time_GetTimePeriodIdForDates",
    value: function (startDate, endDate) {
      startDate = fnDecia_Utility_GetValueAsDate(startDate);
      endDate = fnDecia_Utility_GetValueAsDate(endDate);

      var startDateAsString = fnDecia_Utility_ConvertDateToString(startDate, true, true);
      var endDateAsString = fnDecia_Utility_ConvertDateToString(endDate, true, true);

      var timePeriodId = startDateAsString + "_" + endDateAsString;
      return timePeriodId;
    }
  }
);