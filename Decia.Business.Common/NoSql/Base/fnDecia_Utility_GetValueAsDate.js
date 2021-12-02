MyDb.system.js.save(
  {
    _id: "fnDecia_Utility_GetValueAsDate",
    value: function (value) {
      if (value.constructor === Number) {
        var date = new Date(value);
        return date;
      }
      else if (value.constructor === String) {
        var dateAsText = value.replace(" ", "T");
        dateAsText = (dateAsText.slice(-1) != "Z") ? (dateAsText + "Z") : dateAsText;

        var date = new Date(dateAsText);
        return date;
      }
      else if (value.constructor === Date) {
        var date = new Date(value);
        return date;
      }
      else {
        throw "Invalid date value specified.";
      }
      throw "Encountered unexpected termination while creating date from value.";
    }
  }
);