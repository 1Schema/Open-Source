MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_ConvertNumberToString",
    value: function (number, requiredDigits) {
      var numberAsString = number.toString();
      var sizeDifference = (requiredDigits - numberAsString.length);

      if (sizeDifference <= 0) {
        return numberAsString;
      }

      for (var i = 0; i < sizeDifference; i++) {
        numberAsString = "0" + numberAsString;
      }
      return numberAsString;
    }
  }
);