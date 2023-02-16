local UIBilliardsRule = class("UIBilliardsRule", BaseUIBilliardsRule)

function UIBilliardsRule:OnOpened()
    self.view.lblRule.text = I18nManager.GetCurrentValue("Billiards_RuleTitle", true)
    -- self.view.lblRule_1.text = I18nManager.GetCurrentValue("Billiards_Rule1", true)
    -- self.view.lblRule_2.text = I18nManager.GetCurrentValue("Billiards_Rule2", true)
    -- self.view.lblRule_3.text = I18nManager.GetCurrentValue("Billiards_Rule3", true)
    -- self.view.lblRule_4.text = I18nManager.GetCurrentValue("Billiards_Rule4", true)
    -- self.view.lblRule_5.text = I18nManager.GetCurrentValue("Billiards_Rule5", true)
    -- self.view.lblRule_6.text = I18nManager.GetCurrentValue("Billiards_Rule6", true)
    -- self.view.lblRule_7.text = I18nManager.GetCurrentValue("Billiards_Rule7", true)
    -- self.view.lblRule_8.text = I18nManager.GetCurrentValue("Billiards_Rule8", true)
    self.view.lblSolidBall.text = I18nManager.GetCurrentValue("Billiards_SolidBall", true)
    self.view.lblStripBall.text = I18nManager.GetCurrentValue("Billiards_StripBall", true)
    self.view.lblNormal.text = I18nManager.GetCurrentValue("Billiards_Normal", true)
    self.view.lblEasy.text = I18nManager.GetCurrentValue("Billiards_Easy", true)

    self.view.lblRule_1.text = I18nManager.GetCurrentValue("Billiards_Rule1", true)
    self.view.lblRule_2.text = I18nManager.GetCurrentValue("Billiards_Rule2", true)
    self.view.lblRule_3.text = I18nManager.GetCurrentValue("Billiards_Rule3", true)
    self.view.lblRule_4.text = I18nManager.GetCurrentValue("Billiards_Rule4", true)
    self.view.lblRule_5.text = I18nManager.GetCurrentValue("Billiards_Rule5", true)
    self.view.lblRule_6.text = I18nManager.GetCurrentValue("Billiards_Rule6", true)
    self.view.lblRule_7.text = I18nManager.GetCurrentValue("Billiards_Rule7", true)
    self.view.lblRule_8.text = I18nManager.GetCurrentValue("Billiards_Rule8", true)
end

function UIBilliardsRule:OnButtonClick(com)
    if com == self.view.btnClose then
        self.root:Close(UIBilliardsRule)
    end
end

return UIBilliardsRule