MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_ConvertDateToString",
    value: function (date, includeT, includeZ) {
      includeT = (includeT == true);
      includeZ = (includeZ == true);

      if (date.constructor === Number) {
        date = new Date(date);
      }

      var dateAsString = fnDecia_Utility_ConvertNumberToString(date.getFullYear(), 4) + "-" +
        fnDecia_Utility_ConvertNumberToString(date.getMonth() + 1, 2) + "-" +
        fnDecia_Utility_ConvertNumberToString(date.getDate(), 2) + (includeT ? "T" : " ") +
        fnDecia_Utility_ConvertNumberToString(date.getHours(), 2) + ":" +
        fnDecia_Utility_ConvertNumberToString(date.getMinutes(), 2) + ":" +
        fnDecia_Utility_ConvertNumberToString(date.getSeconds(), 2) + "." +
        fnDecia_Utility_ConvertNumberToString(date.getMilliseconds(), 3) + (includeZ ? "Z" : "");
      return dateAsString;
    }
  }
);